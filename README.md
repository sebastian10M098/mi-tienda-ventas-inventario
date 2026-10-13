# Mi Tienda

Aplicación de escritorio para ventas e inventario de una tienda de repuestos automotrices. Proyecto personal desarrollado con C#, Windows Forms, .NET 8 y MySQL.

## Tecnologías

- C# y .NET 8 Windows Forms
- MySQL y MySqlConnector
- SQL para el esquema, las relaciones y los triggers de inventario

## Funciones

- **Inicio:** indicadores de productos, ventas del día y agotados; gráfico de ventas por período; estado del inventario.
- **Productos:** búsqueda por tipo y filtros de catálogo; agregar y editar productos; costo, precio de venta y control de stock.
- **Inventario:** entradas y salidas registradas en una bitácora mediante triggers de MySQL. El sistema evita borrar productos que tienen movimientos o ventas relacionados.
- **Ventas:** carrito, cálculo de IVA, registro de factura y detalle de productos dentro de una transacción; el stock se descuenta al confirmar.
- **Reportes:** ventas por categoría, productos más vendidos e historial de facturas, filtrados por rangos de fechas.
- **Administración:** acceso a operaciones sensibles validado contra la cuenta de administrador de la base de datos; cambio de contraseña desde Configuración.

Las líneas de venta conservan el precio y el costo unitario vigentes al momento de vender. Las ventas migradas desde el esquema anterior conservan su precio disponible, pero su costo histórico queda en cero porque la base anterior no lo guardaba.

## Estructura

```text
.
├── database/
│   ├── schema.sql       Esquema para instalaciones nuevas
│   └── migrations/      Migraciones manuales para bases anteriores
└── MiTienda/
    ├── Controls/        Pantallas y controles de WinForms
    ├── Data/            Conexión y repositorios MySQL
    ├── Forms/           Formularios modales
    ├── Models/          Modelos de dominio
    └── UI/              Tema visual y componentes reutilizables
```

## Requisitos y ejecución

1. Instala Visual Studio 2022 con la carga de trabajo **Desarrollo de escritorio de .NET** y MySQL 8.
2. En MySQL Workbench, ejecuta `database/schema.sql` para crear una instalación nueva.
3. Copia `MiTienda/appsettings.example.json` como `MiTienda/appsettings.json` y configura el servidor, la base y un usuario MySQL dedicado.
4. Abre `MiTienda.sln` y ejecuta el proyecto `MiTienda`.

Para una instalación local, crea el usuario de la aplicación desde una cuenta administradora de MySQL y reemplaza la clave de ejemplo:

```sql
CREATE USER 'mitienda_app'@'localhost' IDENTIFIED BY 'CAMBIA_ESTA_CLAVE';
GRANT SELECT, INSERT, UPDATE, DELETE ON inventario_db.* TO 'mitienda_app'@'localhost';
```

La aplicación solo verifica la conexión al iniciar. No crea tablas, modifica columnas ni elimina datos de la base automáticamente. Para actualizar una base antigua, haz primero una copia de seguridad y sigue las instrucciones del archivo de migración correspondiente en `database/migrations/`.

## Seguridad y datos de demostración

- `MiTienda/appsettings.json` contiene credenciales locales y está excluido de Git. No publiques ese archivo ni copias de `bin/`.
- El archivo de ejemplo usa un usuario de base de datos dedicado; evita conectar la aplicación con `root`.
- El esquema incluye cuentas de demostración. La contraseña inicial del administrador es `admin123`; cámbiala desde **Configuración** antes de usar datos reales. Las contraseñas nuevas se guardan con PBKDF2; las contraseñas antiguas se convierten al formato hash cuando se validan correctamente.
- Las ventas aún se registran con el usuario administrador de demostración, porque esta versión no incluye una pantalla de inicio de sesión ni selección de usuario.
- Esta versión es una aplicación de portafolio y demostración; no se ha sometido a una auditoría de seguridad ni a validación de requisitos contables o tributarios.

## Migración desde el esquema anterior

Para una base previa que no tenía tabla `factura`, precio unitario ni costo en los productos, revisa `database/migrations/001_invoice_and_price_snapshots.sql`. La migración crea una factura por cada venta antigua porque el esquema previo no guardaba cómo agrupar las líneas. No puede recuperar el costo de ventas históricas.

Haz y verifica una copia de seguridad antes de ejecutarla. Este script es de una sola ejecución y no se debe usar en una base recién creada con `schema.sql`.

## Limitaciones conocidas

- El IVA está fijado en 15 % en la pantalla de ventas.
- No hay inicio de sesión para los empleados ni administración visual de cuentas.
- La zona horaria y moneda deben coincidir con la configuración del servidor MySQL y del equipo.
- No hay suite de pruebas automatizadas ni instalador de distribución en esta versión.
