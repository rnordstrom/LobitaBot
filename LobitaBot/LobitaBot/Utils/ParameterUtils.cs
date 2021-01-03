using System;

namespace LobitaBot
{
    public static class ParameterUtils
    {
        public static string GenerateUniqueParam()
        {
            return $"?_={DateTime.Now.Millisecond}";
        }
    }
}
