Create Database SistemaInventario
USE SistemaInventario;
GO

CREATE TABLE Departamento (
    idDepartamento INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100)
);
GO

CREATE TABLE Rol (
    idRol INT PRIMARY KEY IDENTITY(1,1),
    tipo VARCHAR(100)
);
GO

CREATE TABLE Usuario (
    idUsuario INT PRIMARY KEY IDENTITY(1,1)NOT NULL,
    usuario VARCHAR(100) NOT NULL,
    nombreCompleto VARCHAR(100),
    cedula VARCHAR(20)NOT NULL,
    correo VARCHAR(100)NOT NULL,
    contrasenna NVARCHAR(256) NOT NULL,
    estado BIT NOT NULL,
    idDepartamento INT FOREIGN KEY REFERENCES Departamento(idDepartamento) NOT NULL,
    idRol INT FOREIGN KEY REFERENCES Rol(idRol) NOT NULL
);

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
CREATE OR ALTER PROCEDURE spp_CrearTicket
    @urgencia     VARCHAR(50),
    @detalle      VARCHAR(255),
    @idUsuario    INT,
    @idDepartamento INT
AS
BEGIN
    DECLARE @idResponsable INT;
    DECLARE @rolSoporte    INT;

    -- Asegúrate de que solo trae uno
    SELECT TOP 1 @rolSoporte = idRol
    FROM Rol
    WHERE tipo = 'Soporte';

    SELECT TOP 1 
        @idResponsable = u.idUsuario
    FROM Usuario u
    LEFT JOIN Ticket t 
        ON u.idUsuario = t.idResponsable 
       AND t.solucionado = 0
    WHERE u.idRol = @rolSoporte
    GROUP BY u.idUsuario
    ORDER BY COUNT(t.idTicket) ASC;

    INSERT INTO Ticket 
        (urgencia, detalle, fecha, solucionado, estado, idResponsable, idUsuario, idDepartamento)
    VALUES 
        (@urgencia, @detalle, GETDATE(), 0, 1, @idResponsable, @idUsuario, @idDepartamento);
END
GO


--Consultar ticket por id
CREATE OR ALTER PROCEDURE spp_ConsultarTicket
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
        u.nombreCompleto AS nombreUsuario,
        d.nombre AS nombreDepartamento,
        r.nombreCompleto AS nombreResponsable
    FROM Ticket t
    INNER JOIN Usuario u ON t.idUsuario = u.idUsuario
    INNER JOIN Departamento d ON t.idDepartamento = d.idDepartamento
    LEFT JOIN Usuario r ON t.idResponsable = r.idUsuario
    WHERE t.idTicket = @IdTicket
END;
GO

--Consultar todos los tickets
CREATE OR ALTER PROCEDURE spp_ConsultarTodosTickets
AS
BEGIN
    SELECT 
        t.idTicket, t.urgencia, t.detalle, t.fecha, t.solucionado, t.estado,
        t.detalleTecnico,
		t.idUsuario,
		t.idDepartamento,
		t.idResponsable,
        u.nombreCompleto AS nombreUsuario,
        d.nombre AS nombreDepartamento,
        r.nombreCompleto AS nombreResponsable
    FROM Ticket t
    INNER JOIN Usuario u ON t.idUsuario = u.idUsuario
    INNER JOIN Departamento d ON t.idDepartamento = d.idDepartamento
    LEFT JOIN Usuario r ON t.idResponsable = r.idUsuario
    ORDER BY t.fecha DESC
END

EXEC spp_ConsultarTodosTickets;


--Actualizar ticket
CREATE OR ALTER PROCEDURE spp_ActualizarTicket
  @idTicket INT,
  @urgencia VARCHAR(50),
  @solucionado BIT,
  @detalleTecnico VARCHAR(255),
  @idResponsable INT = NULL
AS
BEGIN
  UPDATE Ticket
  SET urgencia       = @urgencia,
      solucionado    = @solucionado,
      detalleTecnico = @detalleTecnico,
      estado         = CASE WHEN @solucionado = 1 THEN 0 ELSE 1 END,
      idResponsable  = @idResponsable
  WHERE idTicket = @idTicket;
END

-- Listado de Soportes
CREATE OR ALTER PROCEDURE sp_ListarSoportes
AS
BEGIN
    SELECT 
      u.idUsuario,
      u.nombreCompleto
    FROM Usuario u
    INNER JOIN Rol r ON u.idRol = r.idRol
    WHERE r.tipo = 'Soporte'
      AND u.estado = 1;
END
GO

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
@placa INT,
@serie VARCHAR(50),
@descripcion NVARCHAR(1024),
@idDepartamento	INT,
@idResponsable INT)
AS BEGIN

	INSERT INTO Activo(nombreActivo,placa,serie,descripcion,estado,idDepartamento,idResponsable)
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
	D.nombreDepartamento,
	A.idResponsable,
	R.nombreCompleto AS nombreResponsable
	FROM Activo A
	INNER JOIN Departamento D ON D.idDepartamento = A.idDepartamento
	INNER JOIN Usuario R ON R.idUsuario = A.idResponsable
	WHERE A.idActivo = @idActivo
	--AND estado = 1;
END;
GO

--READ (Listado)
CREATE OR ALTER PROCEDURE SPP_ListadoActivo(
@idDepartamento INT NULL
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
	D.nombre,
	A.idResponsable as idResponsable,
	U.idUsuario as idResR,
	U.nombre as nombreResponsable
	FROM Activo A
	INNER JOIN Departamento D ON D.idDepartamento = A.idDepartamento
	INNER JOIN Usuario U ON U.idUsuario = A.idResponsable
	WHERE 1=1';

	IF (@idDepartamento IS NOT NULL AND @idDepartamento <> 0)
	BEGIN
		SET @SQL = @SQL + 'AND A.idDepartamento = @idDepartamento';
	END;

	SET @SQL = @SQL + ' AND A.estado = 1';

	EXEC sp_executesql @SQL,
	N'@idDepartamento INT',
	@idDepartamento = @idDepartamento;
END;
GO

Exec SP_ListadoActivo 1

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

CREATE OR ALTER PROCEDURE SP_ObtenerListaUsuarios
@nombreCompleto VARCHAR(50) = NULL,
@cedula VARCHAR(50) = NULL,
@idDepartamento INT = NULL,
@idRol INT = NULL
AS
BEGIN
	DECLARE @sql NVARCHAR(MAX) = '
	SELECT 
		U.idUsuario,
		U.usuario,
		U.nombreCompleto,
		U.cedula,
		U.correo,
		D.nombreDepartamento,
		R.tipo
	FROM Usuario U
	INNER JOIN Departamento D ON D.idDepartamento = U.idDepartamento
	INNER JOIN Rol R ON R.idRol = U.idRol
	WHERE U.estado = 1'

	IF @nombreCompleto IS NOT NULL AND @nombreCompleto <> ''
		SET @sql += ' AND U.nombreCompleto LIKE ''%' + @nombreCompleto + '%'''

	IF @cedula IS NOT NULL AND @cedula <> ''
		SET @sql += ' AND U.cedula LIKE ''%' + @cedula + '%'''

	IF @idDepartamento IS NOT NULL AND @idDepartamento <> 0
		SET @sql += ' AND U.idDepartamento = ' + CAST(@idDepartamento AS VARCHAR)

	IF @idRol IS NOT NULL AND @idRol <> 0
		SET @sql += ' AND U.idRol = ' + CAST(@idRol AS VARCHAR)

	EXEC sp_executesql @sql
END
GO

CREATE OR ALTER PROCEDURE SP_ObtenerListaDepartamento
AS BEGIN
	SELECT idDepartamento,nombre from Departamento
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

	INSERT INTO Usuario(usuario,nombreCompleto,cedula,correo,contrasenna,idDepartamento,idRol,estado)
	VALUES (@usuario,@nombreCompleto,@cedula,@correo,@contrasenna,@idDepartamento,@idRol,1)

END;
GO
/*
--Registrar cuenta
CREATE OR ALTER PROCEDURE SP_RegistrarCuenta
@usuario VARCHAR(100),
@nombre VARCHAR(100),
@apellido VARCHAR(100),
@cedula VARCHAR(10),
@correo VARCHAR(50),
@contrasenna NVARCHAR(256),
@idDepartamento INT,
@idRol INT
AS BEGIN

<<<<<<< Updated upstream
--READ (Detalle USUARIO)
CREATE OR ALTER PROCEDURE SP_DetallesUsuario(
@idUsuario INT
)
AS
BEGIN

	SELECT 
		U.idUsuario,
		U.usuario,
		U.nombreCompleto,
		U.cedula,
		U.correo,
		D.idDepartamento,
		D.nombreDepartamento,
		R.idRol,
		R.tipo
	FROM Usuario U
	INNER JOIN Departamento D ON D.idDepartamento = U.idDepartamento
	INNER JOIN Rol R ON R.idRol = U.idRol
	WHERE U.idUsuario = @idUsuario

END;
GO

=======
	INSERT INTO Usuario(usuario,nombre, apellido,cedula,correo,contrasenna,idDepartamento,idRol)
	VALUES (@usuario,@nombre, @apellido,@cedula,@correo,@contrasenna,@idDepartamento,@idRol)

END;
GO
*/
>>>>>>> Stashed changes
--Inserts de prueba
INSERT INTO Departamento (nombre) VALUES ('Administración');
INSERT INTO Departamento (nombre) VALUES ('Tecnología');
INSERT INTO Departamento (nombre) VALUES ('Recursos Humanos');
select * from Departamento
-- Insertar roles
INSERT INTO Rol (tipo) VALUES ('Administrador');
INSERT INTO Rol (tipo) VALUES ('Usuario');
INSERT INTO Rol (tipo) VALUES ('Soporte');
Select * from Rol
GO
-- Insertar usuarios
INSERT INTO Usuario (usuario, nombre, apellido, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('jdoe', 'John,',' Doe', '1234567890', 'jdoe@example.com', 'password123', 1, 1, 1);

INSERT INTO Usuario (usuario, nombre, apellido, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('asmith', 'Alice',' Smith', '9876543210', 'asmith@example.com', 'pass456', 1, 2, 2);

INSERT INTO Usuario (usuario, nombre, apellido, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('bgarcia', 'Bob',' Garcia', '1122334455', 'bgarcia@example.com', 'secret789', 1, 3, 3);

INSERT INTO Usuario (usuario, nombre, apellido, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('juanpedro', 'Juan',' Pedro', '1122334465', 'juanpedro@example.com', 'secret7899', 1, 3, 3);

INSERT INTO Usuario (usuario, nombre, apellido, cedula, correo, contrasenna, estado, idDepartamento, idRol)
VALUES ('juanpedro2', 'Juancito',' Pedrito', '1122334475', 'juanpedro2@example.com', 'secret7898', 1, 3, 3);
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
         OR (@estado = 'Solucionados' AND t.solucionado = 1)
         OR (@estado = 'NoSolucionados' AND t.solucionado = 0))
        -- Filtrado por urgencia:
        AND (@urgencia = 'Todos' OR t.urgencia = @urgencia)
    ORDER BY t.fecha DESC;
END;
GO

-- Detalle de Ticket
CREATE OR ALTER PROCEDURE SPP_DetallesTicket
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
        u.nombreCompleto AS nombreUsuario,
        COALESCE(r.nombreCompleto, 'Sin asignar') AS nombreResponsable,
        d.nombre AS nombreDepartamento
    FROM Ticket t
    INNER JOIN Usuario u ON t.idUsuario = u.idUsuario
    INNER JOIN Departamento d ON t.idDepartamento = d.idDepartamento
    LEFT JOIN Usuario r ON t.idResponsable = r.idUsuario
    WHERE t.idTicket = @idTicket;
END

/*
-- Detalle de Ticket CON ROL DE SOPORTE
CREATE OR ALTER PROCEDURE SPP_DetallesTicket
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
        u.nombreCompleto AS nombreUsuario,
        COALESCE(r.nombreCompleto, 'Sin asignar') AS nombreResponsable,
        d.nombre AS nombreDepartamento
    FROM Ticket t
    INNER JOIN Usuario u ON t.idUsuario = u.idUsuario
    LEFT JOIN Usuario r 
        ON t.idResponsable = r.idUsuario 
        AND r.idRol = (SELECT idRol FROM Rol WHERE tipo = 'Soporte')
    INNER JOIN Departamento d ON t.idDepartamento = d.idDepartamento
    WHERE t.idTicket = @idTicket;
END*/
Exec SPP_DetallesTicket 1;


Select * from Ticket

--------- MANTENIMIENTO ----------

--Procedimientos almacenados
--Crear Mantenimiento
CREATE OR ALTER PROCEDURE SP_CrearMantenimiento
    @detalle       VARCHAR(255),
    @idActivo      INT,
    @idUsuario     INT
AS
BEGIN
    DECLARE @idResponsable INT;
    DECLARE @rolSoporte    INT;

    -- Obtener el ID del rol de Soporte
    SELECT TOP 1 @rolSoporte = idRol
    FROM Rol
    WHERE tipo = 'Soporte';

    -- Asignar al soporte con menos mantenimientos activos (estado = 1)
    SELECT TOP 1 
        @idResponsable = u.idUsuario
    FROM Usuario u
    LEFT JOIN Mantenimiento m 
        ON u.idUsuario = m.idResponsable 
        AND m.estado = 1
    WHERE u.idRol = @rolSoporte
    GROUP BY u.idUsuario
    ORDER BY COUNT(m.idMantenimiento) ASC;

    INSERT INTO Mantenimiento
        (fecha, detalle, estado, idResponsable, idActivo, idUsuario)
    VALUES
        (CONVERT(date, GETDATE()), @detalle, 1, @idResponsable, @idActivo, @idUsuario);
END;
GO

--Consultar ticket por id
CREATE OR ALTER PROCEDURE sp_ConsultarMantenimiento
    @IdMantenimiento INT
AS
BEGIN
    SELECT 
       m.idMantenimiento, m.fecha, m.detalle, m.estado,
		m.idUsuario,
		m.idActivo,
		m.idResponsable,
        u.nombre AS nombreUsuario,
        a.nombreActivo AS nombreActivo,
        r.nombre AS nombreResponsable
    FROM Mantenimiento m
    INNER JOIN Usuario u ON m.idUsuario = u.idUsuario
    INNER JOIN Activo a ON m.idActivo = a.idActivo
    LEFT JOIN Usuario r ON m.idResponsable = r.idUsuario
    WHERE m.idMantenimiento = @IdMantenimiento
END;
GO

--Consultar todos los tickets
-- 1) Listar todos los mantenimientos con nombre completo de usuario y responsable
CREATE OR ALTER PROCEDURE SP_ConsultarTodosMantenimientos
AS
BEGIN
    SELECT 
        m.idMantenimiento,
        m.fecha,
        m.detalle,
        m.estado,
        m.idUsuario,
        u.nombreCompleto AS nombreUsuario,
        m.idActivo,
        a.nombreActivo,
        m.idResponsable, r.nombreCompleto AS nombreResponsable
    FROM Mantenimiento m
    INNER JOIN Usuario u ON m.idUsuario        = u.idUsuario
    INNER JOIN Activo  a ON m.idActivo         = a.idActivo
    LEFT  JOIN Usuario r ON m.idResponsable    = r.idUsuario
    ORDER BY m.fecha DESC;
END;
GO

EXEC sp_ConsultarTodosMantenimientos;


--Actualizar ticket
CREATE OR ALTER PROCEDURE sp_ActualizarMantenimiento
 @idMantenimiento INT,
    @fecha           DATE,
    @detalle         VARCHAR(255),
    @estado          BIT,
    @idResponsable   INT,
    @idActivo        INT,
    @idUsuario       INT
AS
BEGIN
    UPDATE Mantenimiento
    SET 
        fecha         = @fecha,
        detalle       = @detalle,
        estado        = @estado,
        idResponsable = @idResponsable,
        idActivo      = @idActivo,
        idUsuario     = @idUsuario
    WHERE idMantenimiento = @idMantenimiento;
END;
GO


CREATE OR ALTER PROCEDURE sp_ActualizarMantenimiento
 @idMantenimiento INT,
    @fecha           DATE,
    @detalle         VARCHAR(255),
    @estado          BIT,
    @idResponsable   INT = NULL,
    @idActivo        INT,
    @idUsuario       INT
AS
BEGIN
    UPDATE Mantenimiento
    SET 
        fecha         = @fecha,
        detalle       = @detalle,
        estado        = @estado,
        idResponsable = @idResponsable,
        idActivo      = @idActivo,
        idUsuario     = @idUsuario
    WHERE idMantenimiento = @idMantenimiento;
END;
GO



CREATE OR ALTER PROCEDURE sp_EliminarMantenimiento
    @idMantenimiento INT
AS
BEGIN
    DELETE FROM Mantenimiento
    WHERE idMantenimiento = @idMantenimiento;
END;
GO

-- SP PARA FILTROS
CREATE OR ALTER PROCEDURE sp_ConsultarTodosMantenimientosFiltro
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
         OR (@estado = 'Solucionados' AND t.solucionado = 1)
         OR (@estado = 'NoSolucionados' AND t.solucionado = 0))
        -- Filtrado por urgencia:
        AND (@urgencia = 'Todos' OR t.urgencia = @urgencia)
    ORDER BY t.fecha DESC;
END;
GO

-- Detalle de Mantenimiento
CREATE OR ALTER PROCEDURE SP_DetallesMantenimiento
    @idMantenimiento INT
AS
BEGIN
    SELECT 
        m.idMantenimiento,
        m.fecha,
        m.detalle,
        m.estado,
        m.idUsuario,
        u.nombreCompleto AS nombreUsuario,
        m.idActivo,
        a.nombreActivo,
        m.idResponsable, r.nombreCompleto AS nombreResponsable
    FROM Mantenimiento m
    INNER JOIN Usuario u ON m.idUsuario        = u.idUsuario
    INNER JOIN Activo  a ON m.idActivo         = a.idActivo
    LEFT  JOIN Usuario r ON m.idResponsable    = r.idUsuario
    WHERE m.idMantenimiento = @idMantenimiento;
END;
GO

Exec SP_DetallesMantenimiento 1

