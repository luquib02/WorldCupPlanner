using Dominio;
using Sistema.DTOs;

namespace Sistema.Mappers;

public static class LogEntryMapper
{
    public static LogEntryDTO ALogEntryDTO(LogEntry logEntry)
    {
        return new LogEntryDTO
        {
            Id = logEntry.Id,
            Timestamp = logEntry.Timestamp,
            UsuarioEmail = logEntry.UsuarioEmail,
            Accion = logEntry.Accion,
            Detalle = logEntry.Detalle
        };
    }

    public static LogEntry ALogEntry(LogEntryDTO logEntryDTO)
    {
        LogEntry logEntry = new LogEntry(logEntryDTO.UsuarioEmail, logEntryDTO.Accion, logEntryDTO.Detalle);
        logEntry.Id = logEntryDTO.Id;
        logEntry.Timestamp = logEntryDTO.Timestamp;
        return logEntry;
    }
}
