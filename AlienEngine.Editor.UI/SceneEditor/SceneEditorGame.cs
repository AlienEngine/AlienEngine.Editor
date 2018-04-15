using AlienEngine.Core.Game;

namespace AlienEngine.Editor.UI.SceneEditor
{
    internal class SceneEditorGame : AlienEngine.Core.Game.Game
    {
        private int _index;

        public SceneEditorGame(SceneEditorScene scene)
        {
            _index = SceneManager.AddScene(scene);
        }

        public override void Start()
        {
            SceneManager.LoadScene(_index);

            base.Start();
        }
    }
}