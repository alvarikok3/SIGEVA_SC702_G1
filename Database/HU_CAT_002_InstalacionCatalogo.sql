-- HU-CAT-002 - Instalacion del catalogo de servicios medicos
-- Ejecutar en la base de datos SIGEVA correspondiente.
-- No elimina tablas ni registros existentes.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Crear categorias solo si no existe la tabla.
    IF OBJECT_ID('dbo.Categorias', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Categorias
        (
            IdCategoria INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            Nombre VARCHAR(100) NOT NULL,
            Descripcion VARCHAR(300) NULL,
            Estado BIT NOT NULL DEFAULT (1),
            FechaRegistro DATETIME NOT NULL DEFAULT (GETDATE())
        );
    END;

    -- Crear servicios solo si no existe la tabla.
    IF OBJECT_ID('dbo.ServiciosMedicos', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ServiciosMedicos
        (
            IdServicio INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            IdCategoria INT NOT NULL,
            Nombre VARCHAR(150) NOT NULL,
            Descripcion VARCHAR(500) NULL,
            Estado BIT NOT NULL DEFAULT (1),
            FechaRegistro DATETIME NOT NULL DEFAULT (GETDATE())
        );
    END;

    -- Registrar categorias medicas sin duplicar nombres.
    INSERT INTO dbo.Categorias (Nombre, Descripcion, Estado, FechaRegistro)
    SELECT V.Nombre, V.Descripcion, 1, GETDATE()
    FROM (VALUES
        ('Fertilidad y reproduccion asistida', 'Servicios de evaluacion y atencion en fertilidad.'),
        ('Ginecologia', 'Servicios especializados en salud ginecologica.'),
        ('Urologia', 'Servicios de atencion urologica.'),
        ('Endocrinologia', 'Servicios de evaluacion y atencion hormonal.'),
        ('Psicologia', 'Servicios de consulta y acompanamiento psicologico.'),
        ('Medicina general', 'Servicios de consulta y atencion medica general.')
    ) AS V(Nombre, Descripcion)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Categorias C
        WHERE C.Nombre COLLATE Latin1_General_100_CI_AI =
              V.Nombre COLLATE Latin1_General_100_CI_AI
    );

    -- Registrar los ocho servicios medicos.
    INSERT INTO dbo.ServiciosMedicos
        (IdCategoria, Nombre, Descripcion, Estado, FechaRegistro)
    SELECT
        C.IdCategoria,
        V.Nombre,
        V.Descripcion,
        1,
        GETDATE()
    FROM (VALUES
        ('Fertilidad y reproduccion asistida', 'Consulta de fertilidad', 'Evaluacion medica inicial de fertilidad.'),
        ('Fertilidad y reproduccion asistida', 'Fecundacion in vitro (FIV)', 'Tratamiento de reproduccion asistida mediante fecundacion in vitro.'),
        ('Fertilidad y reproduccion asistida', 'Inseminacion intrauterina', 'Procedimiento de reproduccion asistida mediante inseminacion intrauterina.'),
        ('Ginecologia', 'Consulta ginecologica', 'Valoracion medica especializada en salud ginecologica.'),
        ('Urologia', 'Consulta urologica', 'Evaluacion medica especializada en urologia.'),
        ('Endocrinologia', 'Consulta de endocrinologia', 'Valoracion de condiciones hormonales y metabolicas.'),
        ('Psicologia', 'Consulta psicologica', 'Atencion y acompanamiento psicologico.'),
        ('Medicina general', 'Consulta de medicina general', 'Atencion medica general y valoracion inicial.')
    ) AS V(Categoria, Nombre, Descripcion)
    INNER JOIN dbo.Categorias C
        ON C.Nombre COLLATE Latin1_General_100_CI_AI =
           V.Categoria COLLATE Latin1_General_100_CI_AI
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ServiciosMedicos S
        WHERE S.IdCategoria = C.IdCategoria
          AND S.Nombre COLLATE Latin1_General_100_CI_AI =
              V.Nombre COLLATE Latin1_General_100_CI_AI
    );

    -- Agregar relacion si no existe ninguna FK equivalente.
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_key_columns FKC
        INNER JOIN sys.foreign_keys FK
            ON FK.object_id = FKC.constraint_object_id
        WHERE FK.parent_object_id = OBJECT_ID('dbo.ServiciosMedicos')
          AND FK.referenced_object_id = OBJECT_ID('dbo.Categorias')
          AND COL_NAME(FKC.parent_object_id, FKC.parent_column_id) = 'IdCategoria'
          AND COL_NAME(FKC.referenced_object_id, FKC.referenced_column_id) = 'IdCategoria'
    )
    BEGIN
        ALTER TABLE dbo.ServiciosMedicos WITH CHECK
        ADD CONSTRAINT FK_ServiciosMedicos_Categorias
        FOREIGN KEY (IdCategoria)
        REFERENCES dbo.Categorias(IdCategoria);
    END;

    COMMIT TRANSACTION;
    PRINT 'HU-CAT-002: Instalacion completada correctamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
