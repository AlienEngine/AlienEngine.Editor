using AlienEngine.Core.Shaders;
using AlienEngine.Core.Shaders.Samples;
using AlienEngine.Shaders;
using AlienEngine.Shaders.ASL;

namespace AlienEngine.Editor.UI.SceneEditor.Shaders
{
    public class ObjectAxisShader : ShaderProgram
    {
        public ObjectAxisShader() : base(new DiffuseVertexShader(), new ObjectAxisFragmentShader())
        {
        }
    }

    [Version("330 core")]
    internal class ObjectAxisFragmentShader : FragmentShader
    {
        [Out] vec4 FragColor;

        // --------------------
        // Material state
        // --------------------
        struct MaterialState
        {
            public int textureTilling;
            public uint blendMode;
            public float bumpScaling;
            public vec4 colorAmbient;
            public vec4 colorDiffuse;
            public vec4 colorEmissive;
            public vec4 colorReflective;
            public vec4 colorSpecular;
            public vec4 colorTransparent;
            public bool hasBlendMode;
            public bool hasBumpScaling;
            public bool hasColorAmbient;
            public bool hasColorDiffuse;
            public bool hasColorEmissive;
            public bool hasColorReflective;
            public bool hasColorSpecular;
            public bool hasColorTransparent;
            public bool hasName;
            public bool hasOpacity;
            public bool hasReflectivity;
            public bool hasShadingMode;
            public bool hasShininess;
            public bool hasShininessStrength;
            public bool hasTextureAmbient;
            public bool hasTextureDiffuse;
            public bool hasTextureDisplacement;
            public bool hasTextureEmissive;
            public bool hasTextureHeight;
            public bool hasTextureLightMap;
            public bool hasTextureNormal;
            public bool hasTextureOpacity;
            public bool hasTextureReflection;
            public bool hasTextureSpecular;
            public bool hasTwoSided;
            public bool hasWireFrame;
            public bool isTwoSided;
            public bool isWireFrameEnabled;
            public float opacity;
            public int propertyCount;
            public float reflectivity;
            public uint shadingMode;
            public float shininess;
            public float shininessStrength;
            public sampler2D textureAmbient;
            public sampler2D textureDiffuse;
            public sampler2D textureDisplacement;
            public sampler2D textureEmissive;
            public sampler2D textureHeight;
            public sampler2D textureLightMap;
            public sampler2D textureNormal;
            public sampler2D textureOpacity;
            public sampler2D textureReflection;
            public sampler2D textureSpecular;
        }

        // Matrial state
        [Uniform] MaterialState materialState;

        void main()
        {
            FragColor = materialState.colorAmbient + materialState.colorDiffuse * 0.5f;
        }
    }
}