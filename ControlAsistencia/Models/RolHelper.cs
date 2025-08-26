using ControlAsistencia.Models.Enums;
using System;

namespace ControlAsistencia.Models
{
    public static class RolHelper
    {
        public static Array Roles => Enum.GetValues(typeof(Rol));
    }
}