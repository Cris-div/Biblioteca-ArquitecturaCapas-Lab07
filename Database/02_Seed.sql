/* Parte 1B - Datos de prueba para BibliotecaDB */
USE BibliotecaDB;
GO

/* Datos de prueba: inserciones repetibles por nombre, ISBN y DNI. */
INSERT INTO dbo.Autores (Nombre, Nacionalidad)
SELECT v.Nombre, v.Nacionalidad
FROM (VALUES
    (N'Gabriel García Márquez', N'Colombiana'),
    (N'Isabel Allende', N'Chilena'),
    (N'Mario Vargas Llosa', N'Peruana'),
    (N'Julio Cortázar', N'Argentina'),
    (N'Jorge Luis Borges', N'Argentina'),
    (N'Jane Austen', N'Británica'),
    (N'George Orwell', N'Británica'),
    (N'Miguel de Cervantes', N'Española')
) AS v(Nombre, Nacionalidad)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Autores a WHERE a.Nombre = v.Nombre);
GO

INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares)
SELECT v.Titulo, v.ISBN, a.AutorId, v.Ejemplares
FROM (VALUES
    (N'Cien años de soledad', '9780307474728', N'Gabriel García Márquez', 4),
    (N'El amor en los tiempos del cólera', '9780307389732', N'Gabriel García Márquez', 4),
    (N'Crónica de una muerte anunciada', '9781400034956', N'Gabriel García Márquez', 3),
    (N'La casa de los espíritus', '9781501117015', N'Isabel Allende', 4),
    (N'Paula', '9780061564908', N'Isabel Allende', 3),
    (N'La ciudad y los perros', '9780060932755', N'Mario Vargas Llosa', 4),
    (N'Conversación en La Catedral', '9780374530493', N'Mario Vargas Llosa', 3),
    (N'La fiesta del Chivo', '9780312428549', N'Mario Vargas Llosa', 3),
    (N'Rayuela', '9780394752846', N'Julio Cortázar', 4),
    (N'Bestiario', '9780394732824', N'Julio Cortázar', 3),
    (N'Ficciones', '9780802130303', N'Jorge Luis Borges', 3),
    (N'El Aleph', '9780142437889', N'Jorge Luis Borges', 3),
    (N'Orgullo y prejuicio', '9780141439518', N'Jane Austen', 3),
    (N'Sentido y sensibilidad', '9780141439662', N'Jane Austen', 3),
    (N'1984', '9780451524935', N'George Orwell', 4),
    (N'Rebelión en la granja', '9780451526342', N'George Orwell', 4),
    (N'Don Quijote de la Mancha I', '9788437604947', N'Miguel de Cervantes', 4),
    (N'Don Quijote de la Mancha II', '9788437604954', N'Miguel de Cervantes', 4),
    (N'El coronel no tiene quien le escriba', '9780307474735', N'Gabriel García Márquez', 3),
    (N'La tregua', '9780307474742', N'Mario Vargas Llosa', 3)
) AS v(Titulo, ISBN, AutorNombre, Ejemplares)
INNER JOIN dbo.Autores a ON a.Nombre = v.AutorNombre
WHERE NOT EXISTS (SELECT 1 FROM dbo.Libros l WHERE l.ISBN = v.ISBN);
GO

INSERT INTO dbo.Socios (DNI, Nombre, Email)
SELECT v.DNI, v.Nombre, v.Email
FROM (VALUES
    ('70000001', N'Ana Torres', N'ana.torres@example.com'),
    ('70000002', N'Luis Mendoza', N'luis.mendoza@example.com'),
    ('70000003', N'Carla Rojas', N'carla.rojas@example.com'),
    ('70000004', N'Mateo Salazar', N'mateo.salazar@example.com'),
    ('70000005', N'Valeria Paredes', N'valeria.paredes@example.com'),
    ('70000006', N'Andrés Vega', N'andres.vega@example.com'),
    ('70000007', N'Lucía Flores', N'lucia.flores@example.com'),
    ('70000008', N'Diego Castillo', N'diego.castillo@example.com'),
    ('70000009', N'Sofía Navarro', N'sofia.navarro@example.com'),
    ('70000010', N'Javier Cabrera', N'javier.cabrera@example.com')
) AS v(DNI, Nombre, Email)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Socios s WHERE s.DNI = v.DNI);
GO

/* Cinco préstamos: el primer socio tiene tres libros pendientes. */
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
SELECT s.SocioId, v.FechaPrestamo, v.FechaLimite, v.Estado
FROM (VALUES
    ('70000001', DATEADD(DAY, -2, CONVERT(DATE, GETDATE())), DATEADD(DAY, 12, CONVERT(DATE, GETDATE())), N'Pendiente'),
    ('70000002', DATEADD(DAY, -20, CONVERT(DATE, GETDATE())), DATEADD(DAY, -6, CONVERT(DATE, GETDATE())), N'Devuelto'),
    ('70000003', DATEADD(DAY, -15, CONVERT(DATE, GETDATE())), DATEADD(DAY, -1, CONVERT(DATE, GETDATE())), N'Devuelto'),
    ('70000004', DATEADD(DAY, -1, CONVERT(DATE, GETDATE())), DATEADD(DAY, 13, CONVERT(DATE, GETDATE())), N'Pendiente'),
    ('70000005', DATEADD(DAY, -10, CONVERT(DATE, GETDATE())), DATEADD(DAY, 4, CONVERT(DATE, GETDATE())), N'Devuelto')
) AS v(DNI, FechaPrestamo, FechaLimite, Estado)
INNER JOIN dbo.Socios s ON s.DNI = v.DNI
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.Prestamos p
    WHERE p.SocioId = s.SocioId AND p.FechaPrestamo = v.FechaPrestamo
);
GO

INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
SELECT p.PrestamoId, l.LibroId, v.FechaDevolucion
FROM (VALUES
    ('70000001', '9780307474728', CAST(NULL AS DATE)),
    ('70000001', '9780141439518', CAST(NULL AS DATE)),
    ('70000001', '9780451524935', CAST(NULL AS DATE)),
    ('70000002', '9780060932755', DATEADD(DAY, -7, CONVERT(DATE, GETDATE()))),
    ('70000003', '9780394752846', DATEADD(DAY, -2, CONVERT(DATE, GETDATE()))),
    ('70000004', '9780802130303', CAST(NULL AS DATE)),
    ('70000005', '9788437604947', DATEADD(DAY, -1, CONVERT(DATE, GETDATE())))
) AS v(DNI, ISBN, FechaDevolucion)
INNER JOIN dbo.Socios s ON s.DNI = v.DNI
INNER JOIN dbo.Prestamos p ON p.SocioId = s.SocioId
    AND p.FechaPrestamo = CASE v.DNI
        WHEN '70000001' THEN DATEADD(DAY, -2, CONVERT(DATE, GETDATE()))
        WHEN '70000002' THEN DATEADD(DAY, -20, CONVERT(DATE, GETDATE()))
        WHEN '70000003' THEN DATEADD(DAY, -15, CONVERT(DATE, GETDATE()))
        WHEN '70000004' THEN DATEADD(DAY, -1, CONVERT(DATE, GETDATE()))
        ELSE DATEADD(DAY, -10, CONVERT(DATE, GETDATE()))
    END
INNER JOIN dbo.Libros l ON l.ISBN = v.ISBN
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.DetallePrestamo d
    WHERE d.PrestamoId = p.PrestamoId AND d.LibroId = l.LibroId
);
GO

/* El stock insertado ya refleja las salidas pendientes de los préstamos de prueba. */

/* Comprobación rápida de cantidades de datos y pendientes. */
SELECT N'Autores' AS Tabla, COUNT(*) AS Cantidad FROM dbo.Autores
UNION ALL SELECT N'Libros', COUNT(*) FROM dbo.Libros
UNION ALL SELECT N'Socios', COUNT(*) FROM dbo.Socios
UNION ALL SELECT N'Prestamos', COUNT(*) FROM dbo.Prestamos
UNION ALL SELECT N'DetallePrestamo', COUNT(*) FROM dbo.DetallePrestamo;
GO

