window.descargarArchivo = (nombreArchivo, contenidoBase64) => {
    const link = document.createElement('a');
    link.href = 'data:application/octet-stream;base64,' + contenidoBase64;
    link.download = nombreArchivo;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};