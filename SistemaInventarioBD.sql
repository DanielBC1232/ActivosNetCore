CREATE TABLE Departamento (
    idDepartamento INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100)
);

CREATE TABLE Rol (
    idRol INT PRIMARY KEY IDENTITY(1,1),
    tipo VARCHAR(100)
);

CREATE TABLE Usuario (
    idUsuario INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    usuario VARCHAR(100) NOT NULL,
    nombre VARCHAR(100),
    apellido VARCHAR(100),
    cedula VARCHAR(20) NOT NULL,
    correo VARCHAR(100) UNIQUE NOT NULL,
    contrasenna NVARCHAR(256) NOT NULL,
    estado BIT DEFAULT 1 NOT NULL,
    idDepartamento INT FOREIGN KEY REFERENCES Departamento(idDepartamento) NOT NULL,
    idRol INT FOREIGN KEY REFERENCES Rol(idRol) NOT NULL
);
GO

CREATE TABLE Usuario (
    idUsuario INT PRIMARY KEY IDENTITY(1,1)NOT NULL,
    usuario VARCHAR(100) NOT NULL,
    nombre VARCHAR(100),
    apellido VARCHAR(100),
    cedula VARCHAR(20)NOT NULL,
    correo VARCHAR(100)NOT NULL,
    contrasenia VARCHAR(100)NOT NULL,
    estado BIT NOT NULL,
    idDepartamento INT FOREIGN KEY REFERENCES Departamento(idDepartamento) NOT NULL,
    idRol INT FOREIGN KEY REFERENCES Rol(idRol) NOT NULL
);

CREATE TABLE Permiso (
    idPermiso INT PRIMARY KEY IDENTITY(1,1),
    tipoPermiso VARCHAR(100)
);

CREATE TABLE Usuario_Permiso (
    id_Usuario_Permiso INT PRIMARY KEY IDENTITY(1,1),
    idUsuario INT FOREIGN KEY REFERENCES Usuario(idUsuario),
    idPermiso INT FOREIGN KEY REFERENCES Permiso(idPermiso)
);

CREATE TABLE Activo (
    idActivo INT PRIMARY KEY IDENTITY(1,1),
    nombreActivo VARCHAR(100),
    placa INT,
    serie VARCHAR(100),
    descripcion VARCHAR(255),
    estado BIT,
    idDepartamento INT FOREIGN KEY REFERENCES Departamento(idDepartamento),
    idUsuario INT FOREIGN KEY REFERENCES Usuario(idUsuario)
);

CREATE TABLE Mantenimiento (
    idMantenimiento INT PRIMARY KEY IDENTITY(1,1),
    fecha DATE,
    detalle VARCHAR(255),
    estado BIT,
    idResponsable INT FOREIGN KEY REFERENCES Usuario(idUsuario),
    idActivo INT FOREIGN KEY REFERENCES Activo(idActivo),
    idUsuario INT FOREIGN KEY REFERENCES Usuario(idUsuario)
);

CREATE TABLE Ticket (
    idTicket INT PRIMARY KEY IDENTITY(1,1),
    urgencia VARCHAR(50),
    detalle VARCHAR(255),
    fecha DATE,
    solucionado BIT,
    estado BIT,
    detalleTecnico VARCHAR(255),
    idResponsable INT FOREIGN KEY REFERENCES Usuario(idUsuario),
    idUsuario INT FOREIGN KEY REFERENCES Usuario(idUsuario),
    idDepartamento INT FOREIGN KEY REFERENCES Departamento(idDepartamento)
);


--Procedimientos almacenados
--Crear ticket 
CREATE PROCEDURE sp_CrearTicket
    @urgencia VARCHAR(50),
    @detalle VARCHAR(255),
    @idUsuario INT,
    @idDepartamento INT
AS
BEGIN
      INSERT INTO Ticket (urgencia, detalle, fecha, solucionado, estado, idResponsable,idUsuario, idDepartamento)
    VALUES (@urgencia, @detalle, GETDATE(), 0, 1, NULL,@idUsuario, @idDepartamento)
END


--Consultar ticket por id
CREATE OR ALTER PROCEDURE sp_ConsultarTicket
    @IdTicket INT
AS
BEGIN
    SELECT 
        t.idTicket, 
        t.urgencia, 
        t.detalle, 
        t.fecha, 
        t.solucionado, 
        t.estado,
        t.detalleTecnico,
        t.idUsuario,
        t.idDepartamento,
        t.idResponsable,
        u.nombre AS nombreUsuario,
        d.nombre AS nombreDepartamento,
        r.nombre AS nombreResponsable
    FROM Ticket t
    INNER JOIN Usuario u ON t.idUsuario = u.idUsuario
    INNER JOIN Departamento d ON t.idDepartamento = d.idDepartamento
    LEFT JOIN Usuario r ON t.idResponsable = r.idUsuario
    WHERE t.idTicket = @IdTicket
END



--Consultar todos los tickets
CREATE OR ALTER PROCEDURE sp_ConsultarTodosTickets
AS
BEGIN
    SELECT 
        t.idTicket, t.urgencia, t.detalle, t.fecha, t.solucionado, t.estado,
        t.detalleTecnico,
		t.idUsuario,
		t.idDepartamento,
		t.idResponsable,
        u.nombre AS nombreUsuario,
        d.nombre AS nombreDepartamento,
        r.nombre AS nombreResponsable
    FROM Ticket t
    INNER JOIN Usuario u ON t.idUsuario = u.idUsuario
    INNER JOIN Departamento d ON t.idDepartamento = d.idDepartamento
    LEFT JOIN Usuario r ON t.idResponsable = r.idUsuario
    ORDER BY t.fecha DESC
END


--Actualizar ticket
CREATE PROCEDURE sp_ActualizarTicket
    @idTicket INT,
    @solucionado BIT,
    @detalleTecnico VARCHAR(255),
    @idResponsable INT
AS
BEGIN
    UPDATE Ticket
    SET solucionado = @solucionado,
        detalleTecnico = @detalleTecnico,
        estado = CASE WHEN @solucionado = 1 THEN 0 ELSE 1 END,
        idResponsable = @idResponsable
    WHERE idTicket = @idTicket
END

--Eliminar ticket
CREATE PROCEDURE sp_EliminarTicket
    @IdTicket INT
AS
BEGIN
    DELETE FROM Ticket WHERE idTicket = @IdTicket
END




--CREATE
CREATE OR ALTER PROCEDURE SP_AgregarActivo(
@nombreActivo VARCHAR(100),
@placa VARCHAR(50),
@serie VARCHAR(50),
@descripcion NVARCHAR(1024),
@idDepartamento	INT,
@idResponsable INT)
AS BEGIN

	INSERT INTO Activo(nombreActivo,placa,serie,descripcion,estado,idDepartamento,idUsuario)
	VALUES (@nombreActivo,@placa,@serie,@descripcion,1,@idDepartamento,@idResponsable);

END;
GO

--READ (Detalle)
CREATE OR ALTER PROCEDURE SP_DetallesActivo(
@idActivo INT
)
AS
BEGIN

	SELECT
	A.idActivo,
	A.nombreActivo,
	A.placa,
	A.serie,
	A.descripcion,
	A.idDepartamento,
	D.nombre AS nombreDepartamento,
	A.idUsuario,
	R.nombre AS nombreResponsable
	FROM Activo A
	INNER JOIN Departamento D ON D.idDepartamento = A.idDepartamento
	INNER JOIN Usuario R ON R.idUsuario = A.idUsuario
	WHERE A.idActivo = @idActivo
	--AND estado = 1;

END;
GO

--READ (Listado)
CREATE OR ALTER PROCEDURE SP_ListadoActivo(
@idDepartamento INT NULL --SelectDepartamento
)
AS
BEGIN

DECLARE @SQL NVARCHAR(MAX);

SET @SQL = 
	'SELECT
	A.idActivo,
	A.nombreActivo,
	A.placa,
	A.serie,
	A.idDepartamento as idDepA,
	D.idDepartamento as idDepD,
	D.nombre AS nombreDepartamento,
	A.idUsuario as idResA,
	R.idUsuario as idResR,
	R.nombre AS nombreResponsable
	FROM Activo A
	INNER JOIN Departamento D ON D.idDepartamento = A.idDepartamento
	INNER JOIN Usuario R ON R.idUsuario = A.idUsuario
	WHERE 1=1';

	IF (@idDepartamento IS NOT NULL)
	BEGIN
		SET @SQL = @SQL + 'AND A.idDepartamento = @idDepartamento';
	END;

	SET @SQL = @SQL + ' AND A.estado = 1';

	EXEC sp_executesql @SQL,
	N'@idDepartamento INT',
	@idDepartamento = @idDepartamento;

END;
GO

--UPDATE
CREATE OR ALTER PROCEDURE SP_EditarActivo(
@idActivo INT,
@nombreActivo VARCHAR(100),
@placa VARCHAR(50),
@serie VARCHAR(50),
@descripcion NVARCHAR(1024),
@idDepartamento	INT,
@idResponsable INT)
AS BEGIN

	UPDATE Activo SET
	nombreActivo = @nombreActivo,
	placa =@placa,
	serie = @serie,
	descripcion = @descripcion,
	idDepartamento = @idDepartamento,
	idUsuario = @idResponsable
	WHERE idActivo = @idActivo

END;
GO

--DELETE
CREATE OR ALTER PROCEDURE SP_EliminarActivo(
@idActivo INT
)
AS BEGIN

	UPDATE Activo SET
	estado = 0
	WHERE idActivo = @idActivo

END;
GO


