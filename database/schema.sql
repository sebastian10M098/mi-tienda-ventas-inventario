-- =========================================================
-- Mi Tienda - inventario_db (esquema del usuario)
-- Ejecutar completo en MySQL Workbench (Ctrl+Shift+Enter)
-- =========================================================

CREATE DATABASE IF NOT EXISTS inventario_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE inventario_db;

-- ---------------------------------------------------------
-- Tablas
-- ---------------------------------------------------------
CREATE TABLE categoria (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nombre_categoria VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255)
);

CREATE TABLE proveedor (
    id_proveedor INT AUTO_INCREMENT PRIMARY KEY,
    nombre_proveedor VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    correo VARCHAR(100),
    direccion VARCHAR(150),
    estado BOOLEAN DEFAULT TRUE
);

CREATE TABLE usuario (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    nombre_usuario VARCHAR(100) NOT NULL,
    correo VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    rol VARCHAR(20) NOT NULL DEFAULT 'EMPLEADO',
    estado BOOLEAN DEFAULT TRUE,
    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE producto (
    id_producto INT AUTO_INCREMENT PRIMARY KEY,
    nombre_producto VARCHAR(100) NOT NULL,
    codigo VARCHAR(100),
    descripcion VARCHAR(255),
    tipo_banda VARCHAR(30),
    tipo_oring VARCHAR(20),
    aplicacion_vehiculo VARCHAR(150),
    marca VARCHAR(100),
    precio_producto DECIMAL(10,2) NOT NULL,
    costo_producto DECIMAL(10,2) NOT NULL DEFAULT 0,
    stock_producto INT NOT NULL DEFAULT 0,
    ubicacion_percha VARCHAR(100),
    id_categoria INT NOT NULL,
    id_proveedor INT NOT NULL,
    FOREIGN KEY (id_categoria) REFERENCES categoria(id_categoria),
    FOREIGN KEY (id_proveedor) REFERENCES proveedor(id_proveedor)
);

CREATE TABLE movimiento (
    id_movimiento INT AUTO_INCREMENT PRIMARY KEY,
    id_producto INT NOT NULL,
    id_usuario INT NOT NULL,
    tipo_movimiento ENUM('ENTRADA', 'SALIDA') NOT NULL,
    cantidad INT NOT NULL,
    stock_anterior INT NOT NULL,
    stock_nuevo INT NOT NULL,
    fecha_movimiento DATETIME DEFAULT CURRENT_TIMESTAMP,
    observacion VARCHAR(255),

    FOREIGN KEY (id_producto) REFERENCES producto(id_producto),
    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

CREATE TABLE factura (
    id_factura INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    valor_iva DECIMAL(10,2) NOT NULL DEFAULT 0,
    total DECIMAL(10,2) NOT NULL,
    fecha_factura DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

CREATE TABLE venta (
    id_venta INT AUTO_INCREMENT PRIMARY KEY,
    id_producto INT NOT NULL,
    id_usuario INT NOT NULL,
    id_factura INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    costo_unitario DECIMAL(10,2) NOT NULL DEFAULT 0,
    aplica_iva BOOLEAN NOT NULL DEFAULT FALSE,
    valor_iva DECIMAL(10,2) NOT NULL DEFAULT 0,
    fecha_venta DATETIME DEFAULT CURRENT_TIMESTAMP,
    observacion VARCHAR(255),

    FOREIGN KEY (id_producto) REFERENCES producto(id_producto),
    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario),
    FOREIGN KEY (id_factura) REFERENCES factura(id_factura)
);

-- ---------------------------------------------------------
-- Triggers
-- ---------------------------------------------------------
DELIMITER $$

CREATE TRIGGER trg_producto_entrada
AFTER INSERT ON producto
FOR EACH ROW
BEGIN
    INSERT INTO movimiento (
        id_producto, id_usuario, tipo_movimiento,
        cantidad, stock_anterior, stock_nuevo, observacion
    )
    VALUES (
        NEW.id_producto, 1, 'ENTRADA',
        NEW.stock_producto, 0, NEW.stock_producto,
        'Stock inicial del producto'
    );
END$$

CREATE TRIGGER trg_producto_aumento_stock
AFTER UPDATE ON producto
FOR EACH ROW
BEGIN
    IF NEW.stock_producto > OLD.stock_producto THEN
        INSERT INTO movimiento (
            id_producto, id_usuario, tipo_movimiento,
            cantidad, stock_anterior, stock_nuevo, observacion
        )
        VALUES (
            NEW.id_producto, 1, 'ENTRADA',
            NEW.stock_producto - OLD.stock_producto,
            OLD.stock_producto, NEW.stock_producto,
            'Aumento de stock'
        );
    END IF;
END$$

CREATE TRIGGER trg_venta
AFTER INSERT ON venta
FOR EACH ROW
BEGIN
    DECLARE v_stock INT;

    SELECT stock_producto INTO v_stock
    FROM producto
    WHERE id_producto = NEW.id_producto;

    IF v_stock < NEW.cantidad THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Stock insuficiente para realizar la venta';
    ELSE
        UPDATE producto
        SET stock_producto = stock_producto - NEW.cantidad
        WHERE id_producto = NEW.id_producto;

        INSERT INTO movimiento (
            id_producto, id_usuario, tipo_movimiento,
            cantidad, stock_anterior, stock_nuevo, observacion
        )
        VALUES (
            NEW.id_producto, NEW.id_usuario, 'SALIDA',
            NEW.cantidad, v_stock, v_stock - NEW.cantidad,
            'Venta de producto'
        );
    END IF;
END$$

DELIMITER ;

-- ---------------------------------------------------------
-- Datos semilla
-- ---------------------------------------------------------

-- Usuario id=1: lo usan los triggers como "usuario del sistema"
-- para registrar entradas de stock automáticas. NO lo borres.
INSERT INTO usuario (nombre_usuario, correo, password, rol) VALUES
    ('Administrador', 'admin@mitienda.com', 'pbkdf2-sha256$210000$SoPW/E/sDelfo7x4zrkZ+w==$hZ8KsP75d9WQyydpzPb4zJRJ/n0wV3YYFyg8gIxciLo=', 'ADMIN'),
    ('Juan Pérez',     'juan@mitienda.com',  'pbkdf2-sha256$210000$wx3OTDxxoIHZHX9fbkKt5A==$HDqruYjALN+OG7zG5NP/mgGP86QYBhsPuX2hQIHw3mY=', 'EMPLEADO');

INSERT INTO categoria (nombre_categoria, descripcion) VALUES
    ('Bandas', 'Bandas para vehículos'),
    ('Orings', 'Orings para vehículos'),
    ('Retenedores', 'Retenedores para vehículos'),
    ('Rulimanes', 'Rulimanes para vehículos');

INSERT INTO proveedor (nombre_proveedor, telefono, correo, direccion) VALUES
    ('TecnoImport S.A.',   '0999123456', 'ventas@tecnoimport.com', 'Av. Principal 123, Guayaquil'),
    ('Distribuidora Andes','0987654321', 'contacto@distandes.com', 'Calle 10 y Av. 5, Quito'),
    ('Suministros Norte',  '0991234567', 'info@suministrosnorte.com', 'Av. Norte 456, Guayaquil');

-- Los productos se cargarán después del conteo e inventario físico.
-- Verificación rápida
SELECT * FROM producto;
SELECT * FROM movimiento ORDER BY id_movimiento;
SELECT * FROM venta;
