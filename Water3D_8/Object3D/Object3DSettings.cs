using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Water3D
{
    public class Object3DSettings
    {
        // standard settings

        private SamplerState stdSamplerState;
        private RasterizerState stdRasterizerState;
        private SamplerState st;
        private RasterizerState rs;
        private BlendState bs;
        private DepthStencilState ds;

        public Object3DSettings()
        {
            // standard values
            stdSamplerState = new SamplerState();
            stdRasterizerState = new RasterizerState();
            st = new SamplerState();
            rs = new RasterizerState();
            bs = StdBlendState;
            ds = StdDepthStencilState;
        }

        public SamplerState StdSamplerState
        {
            get
            {
                return stdSamplerState;
            }
        }

        public SamplerState SamplerState
        {
            get
            {
                return st;
            }

            set
            {
                st = value;
            }
        }

        public RasterizerState StdRasterizerState
        {
            get
            {
                return stdRasterizerState;
            }
        }

        public RasterizerState RasterizerState
        {
            get
            {
                return rs;
            }

            set
            {
                rs = value;
            }
        }

        public BlendState StdBlendState
        {
            get
            {
                return BlendState.AlphaBlend;
            }
        }

        public BlendState BlendState
        {
            get
            {
                return bs;
            }

            set
            {
                bs = value;
            }
        }

        public DepthStencilState StdDepthStencilState
        {
            get
            {
                return DepthStencilState.Default;
            }
        }

        public DepthStencilState DepthStencilState
        {
            get
            {
                return ds;
            }

            set
            {
                ds = value;
            }
        }
    }
}
