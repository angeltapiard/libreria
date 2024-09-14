--Tabla Libros
CREATE TABLE Libros (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(255) NOT NULL,
    autor VARCHAR(255) NOT NULL,
    precio DECIMAL(10, 2) NOT NULL,
    cantidad INT NOT NULL,
    genero VARCHAR(100),
    paginas INT,
    encuadernacion VARCHAR(100),
    portada VARBINARY(MAX)  -- Columna para almacenar la imagen de la portada
);

--Tabla Separadores
CREATE TABLE Separador (
    SeparadorID INT IDENTITY(1,1) PRIMARY KEY,  
    Foto VARBINARY(MAX), 
    Nombre NVARCHAR(100) NOT NULL,  
    Precio DECIMAL(10, 2) NOT NULL,  
    Cantidad INT NOT NULL  
);
