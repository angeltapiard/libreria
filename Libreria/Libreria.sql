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
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255)
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
    Total DECIMAL(10, 2) NOT NULL,
    MetodoPagoID INT FOREIGN KEY REFERENCES MetodosPago(MetodoPagoID),
    Estado NVARCHAR(50) NOT NULL CHECK (Estado IN ('Pendiente', 'Enviado', 'Entregado', 'Cancelado')),
    DireccionEnvio NVARCHAR(255) NOT NULL
);

CREATE TABLE ItemsPedido (
    ItemPedidoID INT IDENTITY(1,1) PRIMARY KEY,
    PedidoID INT FOREIGN KEY REFERENCES Pedidos(PedidoID),
    LibroID INT FOREIGN KEY REFERENCES Libros(id),
    SeparadorID INT FOREIGN KEY REFERENCES Separador(SeparadorID),
    Cantidad INT NOT NULL CHECK (Cantidad > 0)
);

CREATE PROCEDURE RegistrarUsuario
    @Nombre NVARCHAR(100),
    @Apellidos NVARCHAR(100),
    @Email NVARCHAR(255),
    @Contraseña NVARCHAR(255),
    @Telefono NVARCHAR(15),
    @FechaNacimiento DATE,
    @Genero NVARCHAR(10),
    @Rol NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Verificar si el correo ya existe
    IF EXISTS (SELECT 1 FROM Usuarios WHERE Email = @Email)
    BEGIN
        RAISERROR('El correo electrónico ya está en uso.', 16, 1);
        RETURN;
    END

    -- Insertar nuevo usuario
    INSERT INTO Usuarios (Nombre, Apellidos, Email, Contraseña, Telefono, FechaNacimiento, Genero, Rol)
    VALUES (@Nombre, @Apellidos, @Email, @Contraseña, @Telefono, @FechaNacimiento, @Genero, @Rol);
END;