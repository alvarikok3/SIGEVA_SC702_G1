USE SIGEVA_DB;
GO

-- Requiere haber ejecutado el script de actualización SC702.
IF OBJECT_ID('dbo.Permisos', 'U') IS NULL
   OR OBJECT_ID('dbo.RolesPermisos', 'U') IS NULL
BEGIN
    THROW 50001,
        'Primero ejecute el script de actualización SC702.',
        1;
END;
GO

-- Crear el permiso específico para consultar expedientes.
IF NOT EXISTS (
    SELECT 1
    FROM dbo.Permisos
    WHERE NombrePermiso = 'ConsultarHistorialClinico'
)
BEGIN
    INSERT INTO dbo.Permisos
        (NombrePermiso, Descripcion, Estado)
    VALUES
        (
            'ConsultarHistorialClinico',
            'Permite consultar el historial clínico de pacientes.',
            1
        );
END;
GO

-- Comprobar el permiso usando el usuario y su rol actual.
CREATE OR ALTER PROCEDURE dbo.ValidarPermisoHistorialClinico
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.Usuarios U
            INNER JOIN dbo.Roles R
                ON R.IdRol = U.IdRol
            INNER JOIN dbo.RolesPermisos RP
                ON RP.IdRol = R.IdRol
            INNER JOIN dbo.Permisos P
                ON P.IdPermiso = RP.IdPermiso
            WHERE U.IdUsuario = @IdUsuario
              AND U.Estado = 1
              AND R.Estado = 1
              AND P.Estado = 1
              AND P.NombrePermiso = 'ConsultarHistorialClinico'
        )
        THEN 1 ELSE 0 END
    AS BIT) AS TienePermiso;
END;
GO
-- Registros clínicos asociados a un paciente.
IF OBJECT_ID('dbo.RegistrosClinicos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RegistrosClinicos
    (
        IdRegistroClinico INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_RegistrosClinicos PRIMARY KEY,

        IdPaciente INT NOT NULL,
        IdUsuario INT NOT NULL,
        Fecha DATETIME2 NOT NULL,
        Descripcion NVARCHAR(MAX) NOT NULL,

        FechaRegistro DATETIME2 NOT NULL
            CONSTRAINT DF_RegistrosClinicos_FechaRegistro
            DEFAULT SYSDATETIME(),

        CONSTRAINT FK_RegistrosClinicos_Pacientes
            FOREIGN KEY (IdPaciente)
            REFERENCES dbo.Pacientes(IdPaciente),

        CONSTRAINT FK_RegistrosClinicos_Usuarios
            FOREIGN KEY (IdUsuario)
            REFERENCES dbo.Usuarios(IdUsuario)
    );

    CREATE INDEX IX_RegistrosClinicos_PacienteFecha
        ON dbo.RegistrosClinicos(IdPaciente, Fecha);
END;
GO

-- Tratamientos indicados a un paciente.
IF OBJECT_ID('dbo.TratamientosPaciente', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TratamientosPaciente
    (
        IdTratamiento INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_TratamientosPaciente PRIMARY KEY,

        IdPaciente INT NOT NULL,
        IdUsuario INT NOT NULL,
        FechaInicio DATETIME2 NOT NULL,
        FechaFin DATETIME2 NULL,
        Descripcion NVARCHAR(MAX) NOT NULL,

        Estado NVARCHAR(30) NOT NULL
            CONSTRAINT DF_TratamientosPaciente_Estado
            DEFAULT N'Activo',

        FechaRegistro DATETIME2 NOT NULL
            CONSTRAINT DF_TratamientosPaciente_FechaRegistro
            DEFAULT SYSDATETIME(),

        CONSTRAINT FK_TratamientosPaciente_Pacientes
            FOREIGN KEY (IdPaciente)
            REFERENCES dbo.Pacientes(IdPaciente),

        CONSTRAINT FK_TratamientosPaciente_Usuarios
            FOREIGN KEY (IdUsuario)
            REFERENCES dbo.Usuarios(IdUsuario),

        CONSTRAINT CK_TratamientosPaciente_Fechas
            CHECK (
                FechaFin IS NULL
                OR FechaFin >= FechaInicio
            )
    );

    CREATE INDEX IX_TratamientosPaciente_PacienteFecha
        ON dbo.TratamientosPaciente(IdPaciente, FechaInicio);
END;
GO
CREATE OR ALTER PROCEDURE dbo.ConsultarHistorialClinico
    @IdPaciente INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Primer resultado: datos del paciente.
    SELECT
        IdPaciente,
        Nombre AS NombrePaciente,
        Identificacion
    FROM dbo.Pacientes
    WHERE IdPaciente = @IdPaciente;

    -- Segundo resultado: registros del historial.
    SELECT
        H.IdRegistro,
        H.IdPaciente,
        H.Fecha,
        H.TipoRegistro,
        H.Descripcion,
        H.Profesional,
        H.Estado
    FROM
    (
        SELECT
            C.IdCita AS IdRegistro,
            C.IdPaciente,
            CAST(C.FechaHora AS DATETIME2) AS Fecha,
            N'Cita médica' AS TipoRegistro,
            CAST(
                COALESCE(C.Observaciones, N'')
                AS NVARCHAR(MAX)
            ) AS Descripcion,
            COALESCE(U.Nombre, N'No disponible') AS Profesional,
            CAST(C.EstadoCita AS NVARCHAR(30)) AS Estado
        FROM dbo.Citas C
        LEFT JOIN dbo.Usuarios U
            ON U.IdUsuario = C.IdUsuario
        WHERE C.IdPaciente = @IdPaciente

        UNION ALL

        SELECT
            R.IdRegistroClinico,
            R.IdPaciente,
            R.Fecha,
            N'Registro clínico',
            R.Descripcion,
            COALESCE(U.Nombre, N'No disponible'),
            CAST(NULL AS NVARCHAR(30))
        FROM dbo.RegistrosClinicos R
        LEFT JOIN dbo.Usuarios U
            ON U.IdUsuario = R.IdUsuario
        WHERE R.IdPaciente = @IdPaciente

        UNION ALL

        SELECT
            T.IdTratamiento,
            T.IdPaciente,
            T.FechaInicio,
            N'Tratamiento',
            CAST(
                CONCAT(
                    T.Descripcion,
                    CASE
                        WHEN T.FechaFin IS NOT NULL
                        THEN CONCAT(
                            N' | Fecha de finalización: ',
                            CONVERT(NVARCHAR(19), T.FechaFin, 120)
                        )
                        ELSE N''
                    END
                )
                AS NVARCHAR(MAX)
            ),
            COALESCE(U.Nombre, N'No disponible'),
            T.Estado
        FROM dbo.TratamientosPaciente T
        LEFT JOIN dbo.Usuarios U
            ON U.IdUsuario = T.IdUsuario
        WHERE T.IdPaciente = @IdPaciente
    ) H
    ORDER BY
        H.Fecha ASC,
        H.TipoRegistro ASC,
        H.IdRegistro ASC;
END;
GO