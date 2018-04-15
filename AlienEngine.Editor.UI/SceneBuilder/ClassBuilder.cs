using System.IO;
using System.Reflection;

namespace AlienEngine.Editor.UI.SceneBuilder
{
    public static class ClassBuilder
    {
        /// <summary>
        /// Represents the string base for scene class.
        /// </summary>
        private static readonly string SceneClassCodeBase;

        /// <summary>
        /// Represents the string base for a component class.
        /// </summary>
        private static readonly string ComponentClassCodeBase;
        
        static ClassBuilder()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string sceneClassCs = "Assets.Templates.Scene.cs";
            const string componentClassCs = "Assets.Templates.Component.cs";
            
            using (var stream = assembly.GetManifestResourceStream(sceneClassCs))
            using (var reader = new StreamReader(stream))
            {
                SceneClassCodeBase = reader.ReadToEnd();
            }
            
            using (var stream = assembly.GetManifestResourceStream(componentClassCs))
            using (var reader = new StreamReader(stream))
            {
                ComponentClassCodeBase = reader.ReadToEnd();
            }
        }

        public static string CreateSceneClass(string projectName, string className)
        {
            var outputString = SceneClassCodeBase;
            
            outputString = outputString.Replace("${ProjectName}", projectName);
            outputString = outputString.Replace("${ClassName}", className);

            return outputString;
        }

        public static string CreateComponentClass(string projectName, string className)
        {
            var outputString = SceneClassCodeBase;
            
            outputString = outputString.Replace("${ProjectName}", projectName);
            outputString = outputString.Replace("${ClassName}", className);

            return outputString;
        }
    }
}