using System;
using AlienEngine.Core.Graphics;
using AlienEngine.Core.Utils;
using AlienEngine.Editor.UI.Windows.Widgets;

namespace AlienEngine.Editor.UI.Windows.Events
{
    public class SceneEditorAreaEventArgs : System.EventArgs
    {
        #region Constructors

        /// <summary>
        /// Construct a SceneEditorAreaEventArgs.
        /// </summary>
        /// <param name="graphicsContext">
        /// The <see cref="GraphicsContext"/> used for the underlying <see cref="SceneEditorArea"/>.
        /// </param>
        /// <param name="renderContext">
        /// The OpenGL context used for rendering.
        /// </param>
        public SceneEditorAreaEventArgs(GraphicsContext graphicsContext, ContextHandle renderContext)
        {
            if (graphicsContext == null)
                throw new ArgumentNullException(nameof(graphicsContext));
            
            if (renderContext == ContextHandle.Zero)
                throw new ArgumentException("renderContext");

            GraphicsContext = graphicsContext;
            RenderContext = renderContext;
        }

        #endregion

        #region Event Arguments

        /// <summary>
        /// The <see cref="GraphicsContext"/> used for the underlying <see cref="SceneEditorArea"/>.
        /// </summary>
        public readonly GraphicsContext GraphicsContext;

        /// <summary>
        /// The OpenGL context used for rendering.
        /// </summary>
        public readonly ContextHandle RenderContext;

        #endregion
    }
}