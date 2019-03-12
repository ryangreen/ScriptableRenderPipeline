namespace UnityEngine.Rendering.LWRP
{
    [System.Serializable]
    public class StencilStateData
    {
        public bool overrideStencilState = false;
        public int stencilReference = 1;
        public CompareFunction stencilCompareFunction = CompareFunction.Always;
        public StencilOp passOperation = StencilOp.Keep;
        public StencilOp failOperation = StencilOp.Keep;
        public StencilOp zFailOperation = StencilOp.Keep;
    }

    [CreateAssetMenu(fileName = "Custom Forward Renderer", menuName = "Rendering/Lightweight Render Pipeline/Forward Renderer", order = CoreUtils.assetCreateMenuPriority1)]
    public class ForwardRendererData : ScriptableRendererData
    {
        [SerializeField] Shader m_BlitShader = null;
        [SerializeField] Shader m_CopyDepthShader = null;
        [SerializeField] Shader m_ScreenSpaceShadowShader = null;
        [SerializeField] Shader m_SamplingShader = null;

        [SerializeField] LayerMask m_OpaqueLayerMask = -1;
        [SerializeField] LayerMask m_TransparentLayerMask = -1;

        [SerializeField] StencilStateData m_DefaultStencilState = null;

        protected override ScriptableRenderer Create()
        {
            return new ForwardRenderer(this);
        }

        internal Shader blitShader
        {
            get => m_BlitShader;
        }

        internal Shader copyDepthShader
        {
            get => m_CopyDepthShader;
        }

        internal Shader screenSpaceShadowShader
        {
            get => m_ScreenSpaceShadowShader;
        }

        internal Shader samplingShader
        {
            get => m_SamplingShader;
        }

        internal LayerMask opaqueLayerMask
        {
            get => m_OpaqueLayerMask;
        }

        public LayerMask transparentLayerMask
        {
            get => m_TransparentLayerMask;
        }

        public StencilStateData defaultStencilState
        {
            get => m_DefaultStencilState;
        }
    }
}
