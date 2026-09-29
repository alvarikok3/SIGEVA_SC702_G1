using SIGEVA_API.Entities;
using SIGEVA_API.Models;

namespace SIGEVA_API.Interfaces
{
    public interface IUsuarioData
    {
        int RegistrarUsuario(RegistroUsuarioModel model);

        Usuario? ValidarUsuario(LoginModel model);

        List<Rol> ConsultarRoles();

        List<UsuarioConsultaModel> ConsultarUsuarios();

        UsuarioConsultaModel? ConsultarUsuarioPorId(int idUsuario);

        int ActualizarUsuario(UsuarioConsultaModel model);

        int CambiarEstadoUsuario(int idUsuario, bool estado);

        int CambiarContrasena(CambiarContrasenaModel model);
        UsuarioConsultaModel? ConsultarUsuarioPorCorreo(string correo);

        int RestablecerContrasenaTemporal(string correo, string contrasenaTemporal);
    }

}