CREATE TABLE Libros (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(255) NOT NULL,
    autor VARCHAR(255) NOT NULL,
    precio DECIMAL(10, 2) NOT NULL,
    cantidad INT NOT NULL,
    genero VARCHAR(100),
    paginas INT,
    encuadernacion VARCHAR(100),
    portada VARBINARY(MAX)  
);

CREATE TABLE Separador (
    SeparadorID INT IDENTITY(1,1) PRIMARY KEY,  
    Foto VARBINARY(MAX), 
    Nombre NVARCHAR(100) NOT NULL,  
    Precio DECIMAL(10, 2) NOT NULL,  
    Cantidad INT NOT NULL  
);

CREATE TABLE Usuarios (
    UsuarioID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Apellidos NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Contraseña NVARCHAR(255) NOT NULL,
    Telefono NVARCHAR(15), 
    FechaNacimiento DATE,
    Genero NVARCHAR(10) CHECK (Genero IN ('Masculino', 'Femenino')), 
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Admin', 'Cliente')),
    FechaRegistro DATETIME DEFAULT GETDATE()
);

CREATE TABLE MetodosPago (
    MetodoPagoID INT IDENTITY(1,1) PRIMARY KEY,
    TipoTarjeta NVARCHAR(50) NOT NULL,
    NumeroTarjeta NVARCHAR(20) NOT NULL,
    TitularTarjeta NVARCHAR(100) NOT NULL,
    FechaVencimiento DATE NOT NULL,
    CVC NVARCHAR(4) NOT NULL
);

CREATE TABLE Carrito (
    CarritoID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT FOREIGN KEY REFERENCES Usuarios(UsuarioID),
    FechaCreacion DATETIME DEFAULT GETDATE()
);

CREATE TABLE ItemsCarrito (
    ItemCarritoID INT IDENTITY(1,1) PRIMARY KEY,
    CarritoID INT FOREIGN KEY REFERENCES Carrito(CarritoID),
    LibroID INT FOREIGN KEY REFERENCES Libros(id),
    SeparadorID INT FOREIGN KEY REFERENCES Separador(SeparadorID),
    Cantidad INT NOT NULL CHECK (Cantidad > 0)
);

CREATE TABLE Pedidos (
    PedidoID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT FOREIGN KEY REFERENCES Usuarios(UsuarioID),
    FechaPedido DATETIME DEFAULT GETDATE(),
    Calle NVARCHAR(255) NOT NULL,               -- Columna para la calle
    Municipio NVARCHAR(255) NOT NULL,           -- Columna para el municipio
    Provincia NVARCHAR(255) NOT NULL,           -- Columna para la provincia
    Estado NVARCHAR(50) NOT NULL,               -- Nueva columna para el estado del pedido
    Total DECIMAL(18, 2)
                -- Nueva columna para el total del pedido
);



--CREATE PROCEDURE RegistrarUsuario
--    @Nombre NVARCHAR(100),
--    @Apellidos NVARCHAR(100),
--    @Email NVARCHAR(255),
--    @Contraseña NVARCHAR(255),
--    @Telefono NVARCHAR(15),
--    @FechaNacimiento DATE,
--    @Genero NVARCHAR(10),
--    @Rol NVARCHAR(50)
--AS
--BEGIN
--    SET NOCOUNT ON;

--    -- Verificar si el correo ya existe
--    IF EXISTS (SELECT 1 FROM Usuarios WHERE Email = @Email)
--    BEGIN
--        RAISERROR('El correo electrónico ya está en uso.', 16, 1);
--        RETURN;
--    END

--    -- Insertar nuevo usuario
--    INSERT INTO Usuarios (Nombre, Apellidos, Email, Contraseña, Telefono, FechaNacimiento, Genero, Rol)
--    VALUES (@Nombre, @Apellidos, @Email, @Contraseña, @Telefono, @FechaNacimiento, @Genero, @Rol);
--END;



--CREATE PROCEDURE ValidarUsuario
--    @Email NVARCHAR(255),
--    @Contraseña NVARCHAR(255),
--    @Resultado BIT OUTPUT
--AS
--BEGIN
--    SET NOCOUNT ON;

--    DECLARE @Count INT;

--    SELECT @Count = COUNT(*)
--    FROM Usuarios -- Asegúrate de que este sea el nombre correcto de tu tabla de usuarios
--    WHERE Email = @Email AND Contraseña = @Contraseña; -- Asegúrate de que la contraseña esté encriptada si es necesario

--    IF @Count > 0
--        SET @Resultado = 1; -- Usuario válido


--    ELSE
--        SET @Resultado = 0; -- Usuario no válido
--END;



--CREATE PROCEDURE ObtenerRolIDUsuario
--    @Email NVARCHAR(255),
--    @Rol NVARCHAR(50) OUTPUT
--AS
--BEGIN
--    SET NOCOUNT ON;

--    SELECT @Rol = Rol FROM Usuarios WHERE Email = @Email; -- Asegúrate de que 'Rol' sea el nombre correcto de la columna en tu tabla
--END;


--CREATE PROCEDURE ObtenerNombreApellidoUsuario
--    @Email NVARCHAR(100),
--    @Nombre NVARCHAR(50) OUTPUT,
--    @Apellidos NVARCHAR(50) OUTPUT
--AS
--BEGIN
--    SELECT @Nombre = Nombre, @Apellidos = Apellidos
--    FROM Usuarios
--    WHERE Email = @Email;
--END


