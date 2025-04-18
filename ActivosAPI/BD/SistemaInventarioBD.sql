Create Database SistemaInventario
USE SistemaInventario;
GO

CREATE TABLE Departamento (
    idDepartamento INT PRIMARY KEY IDENTITY(1,1),
    nombreDepartamento VARCHAR(100)
);
GO

CREATE TABLE Rol (
    idRol INT PRIMARY KEY IDENTITY(1,1),
    tipo VARCHAR(100)
);
GO

CREATE TABLE Usuario (
    idUsuario INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    usuario VARCHAR(100) NOT NULL,
    nombreCompleto VARCHAR(100) NOT NULL,
    cedula VARCHAR(20) NOT NULL,
    correo VARCHAR(100) UNIQUE NOT NULL,
    contrasenna NVARCHAR(256) NOT NULL,
    estado BIT DEFAULT 1 NOT NULL,
    idDepartamento INT FOREIGN KEY REFERENCES Departamento(idDepartamento) NOT NULL,
    idRol INT FOREIGN KEY REFERENCES Rol(idRol) NOT NULL
);
GO
select * from Usuario
CREATE TABLE Permiso (
    idPermiso INT PRIMARY KEY IDENTITY(1,1),
    tipoPermiso VARCHAR(100)
);
GO

CREATE TABLE Usuario_Permiso (
    id_Usuario_Permiso INT PRIMARY KEY IDENTITY(1,1),
    idUsuario INT FOREIGN KEY REFERENCES Usuario(idUsuario),
    idPermiso INT FOREIGN KEY REFERENCES Permiso(idPermiso)
);
GO

CREATE TABLE Activo (
    idActivo INT PRIMARY KEY IDENTITY(1,1),
    nombreActivo VARCHAR(100),
    placa INT,
    serie VARCHAR(100),
    descripcion VARCHAR(255),
    estado BIT,
    idDepartamento INT FOREIGN KEY REFERENCES Departamento(idDepartamento),
    idResponsable INT FOREIGN KEY REFERENCES Usuario(idUsuario)
);
GO

CREATE TABLE Mantenimiento (
    idMantenimiento INT PRIMARY KEY IDENTITY(1,1),
    fecha DATE,
    detalle VARCHAR(255),
    estado BIT,
    idResponsable INT FOREIGN KEY REFERENCES Usuario(idUsuario),
    idActivo INT FOREIGN KEY REFERENCES Activo(idActivo),
    idUsuario INT FOREIGN KEY REFERENCES Usuario(idUsuario)
);
GO

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
GO
--Procedimientos almacenados
--Crear ticket 
CREATE OR ALTER PROCEDURE sp_CrearTicket
    @urgencia VARCHAR(50),
    @detalle VARCHAR(255),
    @idUsuario INT,
    @idDepartamento INT
AS
BEGIN
      INSERT INTO Ticket (urgencia, detalle, fecha, solucionado, estado, idResponsable,idUsuario, idDepartamento)
    VALUES (@urgencia, @detalle, GETDATE(), 0, 1, NULL,@idUsuario, @idDepartamento)
END;
GO

--Crear ticket ACTUALIZADO
CREATE OR ALTER PROCEDURE sp_CrearTicket
    @urgencia VARCHAR(50),
    @detalle VARCHAR(255),
    @idUsuario INT,
    @idDepartamento INT
AS
BEGIN
	DECLARE @idResponsable INT

	SELECT TOP 1 @idResponsable = u.idUsuario
	FROM Usuario u
	LEFT JOIN Ticket t ON u.idUsuario = t.idResponsable AND t.solucionado = 0
	WHERE u.idRol = (SELECT idRol FROM Rol WHERE tipo = 'Soporte')
	GROUP BY u.idUsuario
	ORDER BY COUNT(t.idTicket) ASC;


    INSERT INTO Ticket (urgencia, detalle, fecha, solucionado, estado, idResponsable,idUsuario, idDepartamento)
    VALUES (@urgencia, @detalle, GETDATE(), 0, 1, @idResponsable,@idUsuario, @idDepartamento)
END;
GO

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
END;
GO

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
GO

--Actualizar ticket
CREATE OR ALTER PROCEDURE sp_ActualizarTicket
    @idTicket INT,
	@urgencia VARCHAR(50),
    @solucionado BIT,
    @detalleTecnico VARCHAR(255),
    @idResponsable INT
AS
BEGIN
    UPDATE Ticket
    SET 
		urgencia = @urgencia,
		solucionado = @solucionado,
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
GO

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
	A.idResponsable,
	R.nombre AS nombreResponsable
	FROM Activo A
	INNER JOIN Departamento D ON D.idDepartamento = A.idDepartamento
	INNER JOIN Usuario R ON R.idUsuario = A.idResponsable
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
	D.nombreDepartamento,
	A.idResponsable as idResponsable,
	U.idUsuario as idResR,
	U.nombreCompleto as nombreResponsable
	FROM Activo A
	INNER JOIN Departamento D ON D.idDepartamento = A.idDepartamento
	INNER JOIN Usuario U ON U.idUsuario = A.idResponsable
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
	@idResponsable = @idResponsable
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

CREATE OR ALTER PROCEDURE SP_IniciarSesion
@correo VARCHAR(50),
@contrasenna VARCHAR(256)
AS BEGIN

	SELECT
		U.idUsuario,
		U.usuario,
		U.idRol,
		R.tipo
	FROM Usuario U
	INNER JOIN Rol R ON R.idRol = U.idRol
	WHERE U.correo = @correo
	AND contrasenna = @contrasenna

END;
GO

--REGISTRAR CUENTA
CREATE OR ALTER PROCEDURE SP_RegistrarCuenta
@usuario VARCHAR(100),
@nombreCompleto VARCHAR(100),
@cedula VARCHAR(10),
@correo VARCHAR(50),
@contrasenna NVARCHAR(256),
@idDepartamento INT,
@idRol INT
AS BEGIN

	INSERT INTO Usuario(usuario,nombreCompleto,cedula,correo,contrasenna,idDepartamento,idRol)
	VALUES (@usuario,@nombreCompleto,@cedula,@correo,@contrasenna,@idDepartamento,@idRol)

END;
GO

--Inserts de prueba
INSERT INTO Departamento (nombreDepartamento) VALUES ('Administración');
INSERT INTO Departamento (nombreDepartamento) VALUES ('Tecnología');
INSERT INTO Departamento (nombreDepartamento) VALUES ('Recursos Humanos');
select * from Departamento
-- Insertar roles
INSERT INTO Rol (tipo) VALUES ('Administrador');
INSERT INTO Rol (tipo) VALUES ('Usuario');
INSERT INTO Rol (tipo) VALUES ('Soporte');
Select * from Rol
GO
-- Insertar usuarios
INSERT INTO Usuario (usuario, nombreCompleto, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('jdoe', 'John Doe', '1234567890', 'jdoe@example.com', 'password123', 1, 1, 1);

INSERT INTO Usuario (usuario, nombreCompleto, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('asmith', 'Alice Smith', '9876543210', 'asmith@example.com', 'pass456', 1, 2, 2);

INSERT INTO Usuario (usuario, nombreCompleto, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('bgarcia', 'Bob Garcia', '1122334455', 'bgarcia@example.com', 'secret789', 1, 3, 3);

INSERT INTO Usuario (usuario, nombreCompleto, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('juanpedro', 'Juan Pedro', '1122334465', 'juanpedro@example.com', 'secret7899', 1, 3, 3);

INSERT INTO Usuario (usuario, nombreCompleto, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('juanpedro2', 'Juancito Pedrito', '1122334475', 'juanpedro2@example.com', 'secret7898', 1, 3, 3);
GO

-- Insertar permisos
INSERT INTO Permiso (tipoPermiso) VALUES ('Crear');
INSERT INTO Permiso (tipoPermiso) VALUES ('Editar');
INSERT INTO Permiso (tipoPermiso) VALUES ('Eliminar');

-- Insertar usuario_permiso (relacionando usuarios con permisos)
INSERT INTO Usuario_Permiso (idUsuario, idPermiso) VALUES (1, 1);
INSERT INTO Usuario_Permiso (idUsuario, idPermiso) VALUES (1, 2);
INSERT INTO Usuario_Permiso (idUsuario, idPermiso) VALUES (2, 2);
INSERT INTO Usuario_Permiso (idUsuario, idPermiso) VALUES (3, 3);
GO
-- Insertar activos
INSERT INTO Activo (nombreActivo, placa, serie, descripcion, estado, idDepartamento, idResponsable)
VALUES ('Laptop Dell', 12345, 'SN12345', 'Laptop para uso general', 1, 2, 1);

INSERT INTO Activo (nombreActivo, placa, serie, descripcion, estado, idDepartamento, idResponsable)
VALUES ('Impresora HP', 67890, 'SN67890', 'Impresora multifunción', 1, 1, 2);

INSERT INTO Activo (nombreActivo, placa, serie, descripcion, estado, idDepartamento, idResponsable)
VALUES ('Monitor Samsung', 11111, 'SN11111', 'Monitor de alta definición', 1, 2, 3);
GO

-- Insertar mantenimientos
INSERT INTO Mantenimiento (fecha, detalle, estado, idResponsable, idActivo, idUsuario)
VALUES ('2025-01-10', 'Cambio de batería', 1, 1, 1, 1);

INSERT INTO Mantenimiento (fecha, detalle, estado, idResponsable, idActivo, idUsuario)
VALUES ('2025-02-15', 'Revisión de impresora', 1, 2, 2, 2);

INSERT INTO Mantenimiento (fecha, detalle, estado, idResponsable, idActivo, idUsuario)
VALUES ('2025-03-20', 'Ajuste de monitor', 1, 3, 3, 3);

-- Insertar tickets
INSERT INTO Ticket (urgencia, detalle, fecha, solucionado, estado, detalleTecnico, idResponsable, idUsuario, idDepartamento)
VALUES ('Alta', 'Problema con el equipo', '2025-04-01', 0, 1, 'Pendiente de revisión', 1, 1, 2);

INSERT INTO Ticket (urgencia, detalle, fecha, solucionado, estado, detalleTecnico, idResponsable, idUsuario, idDepartamento)
VALUES ('Media', 'Error en software', '2025-04-05', 0, 1, 'En proceso de diagnóstico', 2, 2, 1);

INSERT INTO Ticket (urgencia, detalle, fecha, solucionado, estado, detalleTecnico, idResponsable, idUsuario, idDepartamento)
VALUES ('Baja', 'Solicitud de actualización', '2025-04-10', 1, 1, 'Actualizado correctamente', 3, 3, 3);
GO
Select * from Ticket;

-- SP PARA FILTROS
CREATE OR ALTER PROCEDURE sp_ConsultarTodosTicketsFiltro
    @estado VARCHAR(50) = 'Todos',  -- 'Todos', 'Solucionados' o 'NoSolucionados'
    @urgencia VARCHAR(50) = 'Todos'  -- 'Todos', 'Baja', 'Media', 'Alta'
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
    WHERE 
        -- Filtrado por estado:
        (@estado = 'Todos' 
         OR (@estado = 'Solucionados' AN t.solucionado = 1)
         OR (@estado = 'NoSolucionados' AND t.solucionado = 0))
        -- Filtrado por urgencia:
        AND (@urgencia = 'Todos' OR t.urgencia = @urgencia)
    ORDER BY t.fecha DESC;
END;
GO

-- Detalle de Ticket
CREATE OR ALTER PROCEDURE SP_DetallesTicket
    @idTicket INT
AS
BEGIN
    SELECT 
        t.idTicket,
        t.urgencia,
        t.detalle,
        t.fecha,
        t.solucionado,
		t.estado,
		t.idUsuario,
		t.idDepartamento,
		t.idResponsable,
        t.detalleTecnico,
        u.nombre AS nombreUsuario,
        COALESCE(r.nombre, 'Sin asignar') AS nombreResponsable,
        d.nombre AS nombreDepartamento
    FROM Ticket t
    INNER JOIN Usuario u ON t.idUsuario = u.idUsuario
    LEFT JOIN Usuario r 
        ON t.idResponsable = r.idUsuario 
        AND r.idRol = (SELECT idRol FROM Rol WHERE tipo = 'Soporte')
    INNER JOIN Departamento d ON t.idDepartamento = d.idDepartamento
    WHERE t.idTicket = @idTicket;
END

Exec SP_DetallesTicket 11;


Select * from Ticket