-- Migración manual desde el esquema anterior, que guardaba ventas sin factura.
-- Haz una copia de seguridad antes de ejecutar. Aplicar una sola vez.
-- No ejecutar sobre una base recién creada con database/schema.sql.
USE inventario_db;

ALTER TABLE producto
    ADD COLUMN costo_producto DECIMAL(10,2) NOT NULL DEFAULT 0 AFTER precio_producto;

-- Algunas instalaciones anteriores ya tenían tipo_oring. Agrégala solo si falta.
SET @agregar_tipo_oring = IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'producto' AND COLUMN_NAME = 'tipo_oring') = 0,
    'ALTER TABLE producto ADD COLUMN tipo_oring VARCHAR(20) NULL AFTER tipo_banda',
    'SELECT 1'
);
PREPARE migracion_tipo_oring FROM @agregar_tipo_oring;
EXECUTE migracion_tipo_oring;
DEALLOCATE PREPARE migracion_tipo_oring;

ALTER TABLE venta
    ADD COLUMN precio_unitario DECIMAL(10,2) NULL AFTER cantidad,
    ADD COLUMN costo_unitario DECIMAL(10,2) NOT NULL DEFAULT 0 AFTER precio_unitario,
    ADD COLUMN aplica_iva BOOLEAN NOT NULL DEFAULT FALSE AFTER costo_unitario,
    ADD COLUMN valor_iva DECIMAL(10,2) NOT NULL DEFAULT 0 AFTER aplica_iva,
    ADD COLUMN id_factura INT NULL AFTER id_usuario;

CREATE TABLE factura (
    id_factura INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    valor_iva DECIMAL(10,2) NOT NULL DEFAULT 0,
    total DECIMAL(10,2) NOT NULL,
    fecha_factura DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

-- Conserva el precio disponible durante la migración. El costo histórico
-- no se puede reconstruir; por eso las ventas antiguas quedan con costo 0.
UPDATE venta v
JOIN producto p ON p.id_producto = v.id_producto
SET v.precio_unitario = p.precio_producto
WHERE v.precio_unitario IS NULL;

-- La tabla factura no existía en el esquema anterior. Se genera una factura
-- por cada fila antigua de venta, porque no existe información para reagruparlas.
SET @id_factura_base = (SELECT COALESCE(MAX(id_factura), 0) FROM factura);
UPDATE venta
SET id_factura = @id_factura_base + id_venta
WHERE id_factura IS NULL;

INSERT INTO factura (id_factura, id_usuario, subtotal, valor_iva, total, fecha_factura)
SELECT v.id_factura,
       MIN(v.id_usuario),
       SUM(v.cantidad * v.precio_unitario),
       SUM(v.valor_iva),
       SUM(v.cantidad * v.precio_unitario + v.valor_iva),
       MIN(v.fecha_venta)
FROM venta v
GROUP BY v.id_factura;

ALTER TABLE venta
    MODIFY COLUMN precio_unitario DECIMAL(10,2) NOT NULL,
    MODIFY COLUMN id_factura INT NOT NULL,
    ADD CONSTRAINT fk_venta_factura FOREIGN KEY (id_factura) REFERENCES factura(id_factura);
