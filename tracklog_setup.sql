-- Tracklog: solo tabla de auditoría de envíos (sin flag de habilitar/deshabilitar
-- en device — ese módulo por ahora es únicamente de consulta).
CREATE TABLE IF NOT EXISTS auditoriatracklog (
  id INT AUTO_INCREMENT PRIMARY KEY,
  accountID VARCHAR(50) NOT NULL,
  deviceID VARCHAR(50) NOT NULL,
  fecharegistro DATETIME NOT NULL,
  lastenvio TEXT NULL,
  lastrespuesta TEXT NULL,
  INDEX idx_auditoriatracklog_account_device (accountID, deviceID),
  INDEX idx_auditoriatracklog_fecha (fecharegistro)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
