/*
    Parte 1 - Base de datos BibliotecaDB
    Compatible con SQL Server / LocalDB.
    Ejecutar este archivo en SQL Server Management Studio o con sqlcmd.
*/

IF DB_ID(N'BibliotecaDB') IS NULL
BEGIN
    CREATE DATABASE BibliotecaDB;
END;
GO

USE BibliotecaDB;
GO

IF OBJECT_ID(N'dbo.Autores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Autores
    (
        AutorId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Autores PRIMARY KEY,
        Nombre        NVARCHAR(120) NOT NULL,
        Nacionalidad  NVARCHAR(80) NULL,
        Activo        BIT NOT NULL CONSTRAINT DF_Autores_Activo DEFAULT (1)
    );
END;
GO

IF OBJECT_ID(N'dbo.Libros', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Libros
    (
        LibroId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Libros PRIMARY KEY,
        Titulo     NVARCHAR(180) NOT NULL,
        ISBN       VARCHAR(20) NOT NULL,
        AutorId    INT NOT NULL,
        Ejemplares INT NOT NULL,
        Activo     BIT NOT NULL CONSTRAINT DF_Libros_Activo DEFAULT (1),
        CONSTRAINT UQ_Libros_ISBN UNIQUE (ISBN),
        CONSTRAINT CK_Libros_Ejemplares_NoNegativos CHECK (Ejemplares >= 0),
        CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES dbo.Autores(AutorId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Socios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Socios
    (
        SocioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Socios PRIMARY KEY,
        DNI     VARCHAR(15) NOT NULL,
        Nombre  NVARCHAR(140) NOT NULL,
        Email   NVARCHAR(160) NULL,
        Activo  BIT NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT (1),
        CONSTRAINT UQ_Socios_DNI UNIQUE (DNI)
    );
END;
GO

IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Prestamos
    (
        PrestamoId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prestamos PRIMARY KEY,
        SocioId       INT NOT NULL,
        FechaPrestamo DATE NOT NULL,
        FechaLimite   DATE NOT NULL,
        Estado        NVARCHAR(20) NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT (N'Pendiente'),
        CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN (N'Pendiente', N'Devuelto')),
        CONSTRAINT CK_Prestamos_Fechas CHECK (FechaLimite >= FechaPrestamo),
        CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES dbo.Socios(SocioId)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetallePrestamo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetallePrestamo
    (
        PrestamoId      INT NOT NULL,
        LibroId         INT NOT NULL,
        FechaDevolucion DATE NULL,
        CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId),
        CONSTRAINT FK_DetallePrestamo_Prestamos FOREIGN KEY (PrestamoId) REFERENCES dbo.Prestamos(PrestamoId),
        CONSTRAINT FK_DetallePrestamo_Libros FOREIGN KEY (LibroId) REFERENCES dbo.Libros(LibroId)
    );
END;
GO

