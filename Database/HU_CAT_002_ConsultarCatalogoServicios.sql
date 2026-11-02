-- HU-CAT-002 - Catalogo de servicios medicos

CREATE OR ALTER PROCEDURE dbo.ConsultarCatalogoServicios
    @Busqueda NVARCHAR(150) = NULL,
    @IdCategoria INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @Busqueda = NULLIF(LTRIM(RTRIM(@Busqueda)), '');

    SELECT
        S.IdServicio,
        S.IdCategoria,
        S.Nombre,
        S.Descripcion,
        S.Estado,
        C.Nombre AS Categoria
    FROM dbo.ServiciosMedicos AS S
    INNER JOIN dbo.Categorias AS C
        ON S.IdCategoria = C.IdCategoria
    WHERE
        S.Estado = 1
        AND C.Estado = 1
        AND (
            @Busqueda IS NULL
            OR S.Nombre LIKE '%' + @Busqueda + '%'
            OR C.Nombre LIKE '%' + @Busqueda + '%'
        )
        AND (
            @IdCategoria IS NULL
            OR S.IdCategoria = @IdCategoria
        )
    ORDER BY C.Nombre, S.Nombre;
END;
