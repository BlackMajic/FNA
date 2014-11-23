#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
	{
		#region Public Properties

		private EffectTechnique INTERNAL_currentTechnique;
		public EffectTechnique CurrentTechnique
		{
			get
			{
				return INTERNAL_currentTechnique;
			}
			set
			{
				MojoShader.MOJOSHADER_effectSetTechnique(
					glEffect.EffectData,
					value.TechniquePointer
				);
				INTERNAL_currentTechnique = value;
			}
		}

		public EffectParameterCollection Parameters
		{
			get;
			private set;
		}

		public EffectTechniqueCollection Techniques
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private OpenGLDevice.OpenGLEffect glEffect;
		private Dictionary<string, EffectParameter> samplerMap = new Dictionary<string, EffectParameter>();

		#endregion

		#region Private Static Variables

		private Dictionary<MojoShader.MOJOSHADER_symbolType, EffectParameterType> XNAType =
			new Dictionary<MojoShader.MOJOSHADER_symbolType, EffectParameterType>()
		{
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_VOID,
				EffectParameterType.Void
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_BOOL,
				EffectParameterType.Bool
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_INT,
				EffectParameterType.Int32
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_FLOAT,
				EffectParameterType.Single
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_STRING,
				EffectParameterType.String
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE,
				EffectParameterType.Texture
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE1D,
				EffectParameterType.Texture1D
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE2D,
				EffectParameterType.Texture2D
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE3D,
				EffectParameterType.Texture3D
			},
			{
				MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURECUBE,
				EffectParameterType.TextureCube
			},
		};

		private static Dictionary<MojoShader.MOJOSHADER_symbolClass, EffectParameterClass> XNAClass =
			new Dictionary<MojoShader.MOJOSHADER_symbolClass, EffectParameterClass>()
		{
			{
				MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_SCALAR,
				EffectParameterClass.Scalar
			},
			{
				MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_VECTOR,
				EffectParameterClass.Vector
			},
			{
				MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_MATRIX_ROWS,
				EffectParameterClass.Matrix
			},
			{
				MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_MATRIX_COLUMNS,
				EffectParameterClass.Matrix
			},
			{
				MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_OBJECT,
				EffectParameterClass.Object
			},
			{
				MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_STRUCT,
				EffectParameterClass.Struct
			}
		};

		private static readonly Dictionary<MojoShader.MOJOSHADER_blendMode, Blend> XNABlend =
			new Dictionary<MojoShader.MOJOSHADER_blendMode, Blend>()
		{
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_ZERO,		Blend.Zero },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_ONE,			Blend.One },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_SRCCOLOR,		Blend.SourceColor },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_INVSRCCOLOR,		Blend.InverseSourceColor },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_SRCALPHA,		Blend.SourceAlpha },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_INVSRCALPHA,		Blend.InverseSourceAlpha },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_DESTALPHA,		Blend.DestinationAlpha },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_INVDESTALPHA,	Blend.InverseDestinationAlpha },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_DESTCOLOR,		Blend.DestinationColor },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_INVDESTCOLOR,	Blend.InverseDestinationColor },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_SRCALPHASAT,		Blend.SourceAlphaSaturation },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_BLENDFACTOR,		Blend.BlendFactor },
			{ MojoShader.MOJOSHADER_blendMode.MOJOSHADER_BLEND_INVBLENDFACTOR,	Blend.InverseBlendFactor },
		};

		private static readonly Dictionary<MojoShader.MOJOSHADER_blendOp, BlendFunction> XNABlendOp =
			new Dictionary<MojoShader.MOJOSHADER_blendOp, BlendFunction>()
		{
			{ MojoShader.MOJOSHADER_blendOp.MOJOSHADER_BLENDOP_ADD,		BlendFunction.Add },
			{ MojoShader.MOJOSHADER_blendOp.MOJOSHADER_BLENDOP_SUBTRACT,	BlendFunction.Subtract },
			{ MojoShader.MOJOSHADER_blendOp.MOJOSHADER_BLENDOP_REVSUBTRACT,	BlendFunction.ReverseSubtract },
			{ MojoShader.MOJOSHADER_blendOp.MOJOSHADER_BLENDOP_MIN,		BlendFunction.Min },
			{ MojoShader.MOJOSHADER_blendOp.MOJOSHADER_BLENDOP_MAX,		BlendFunction.Max }
		};

		private static readonly Dictionary<MojoShader.MOJOSHADER_compareFunc, CompareFunction> XNACompare =
			new Dictionary<MojoShader.MOJOSHADER_compareFunc, CompareFunction>()
		{
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_NEVER,		CompareFunction.Never },
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_LESS,		CompareFunction.Less },
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_EQUAL,		CompareFunction.Equal },
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_LESSEQUAL,		CompareFunction.LessEqual },
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_GREATER,		CompareFunction.Greater },
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_NOTEQUAL,		CompareFunction.NotEqual },
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_GREATEREQUAL,	CompareFunction.GreaterEqual },
			{ MojoShader.MOJOSHADER_compareFunc.MOJOSHADER_CMP_ALWAYS,		CompareFunction.Always }
		};

		private static readonly Dictionary<MojoShader.MOJOSHADER_stencilOp, StencilOperation> XNAStencilOp =
			new Dictionary<MojoShader.MOJOSHADER_stencilOp, StencilOperation>()
		{
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_KEEP,	StencilOperation.Keep },
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_ZERO,	StencilOperation.Zero },
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_REPLACE,	StencilOperation.Replace },
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_INCRSAT,	StencilOperation.IncrementSaturation },
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_DECRSAT,	StencilOperation.DecrementSaturation },
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_INVERT,	StencilOperation.Invert },
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_INCR,	StencilOperation.Increment },
			{ MojoShader.MOJOSHADER_stencilOp.MOJOSHADER_STENCILOP_DECR,	StencilOperation.Decrement }
		};

		private static readonly Dictionary<MojoShader.MOJOSHADER_textureAddress, TextureAddressMode> XNAAddress =
			new Dictionary<MojoShader.MOJOSHADER_textureAddress, TextureAddressMode>()
		{
			{ MojoShader.MOJOSHADER_textureAddress.MOJOSHADER_TADDRESS_WRAP,	TextureAddressMode.Wrap },
			{ MojoShader.MOJOSHADER_textureAddress.MOJOSHADER_TADDRESS_MIRROR,	TextureAddressMode.Mirror },
			{ MojoShader.MOJOSHADER_textureAddress.MOJOSHADER_TADDRESS_CLAMP,	TextureAddressMode.Clamp }
		};

		#endregion

		#region Public Constructor

		public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
		{
			GraphicsDevice = graphicsDevice;

			// Send the blob to the GLDevice to be parsed/compiled
			glEffect = graphicsDevice.GLDevice.CreateEffect(effectCode);

			// This is where it gets ugly...
			INTERNAL_parseEffectStruct();

			// The default technique is the first technique.
			CurrentTechnique = Techniques[0];
		}

		#endregion

		#region Protected Constructor

		protected Effect(Effect cloneSource)
		{
			// FIXME: MojoShader needs MOJOSHADER_effectClone! -flibit
			throw new NotImplementedException("See MojoShader!");
		}

		#endregion

		#region Public Methods

		public virtual Effect Clone()
		{
			return new Effect(this);
		}

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed && disposing)
			{
				GraphicsDevice.GLDevice.DeleteEffect(glEffect);
			}
			base.Dispose(disposing);
		}

		protected internal virtual void OnApply()
		{
		}

		#endregion

		#region Internal Methods

		internal unsafe void INTERNAL_applyEffect(uint pass)
		{
			MojoShader.MOJOSHADER_effectStateChanges stateChanges = new MojoShader.MOJOSHADER_effectStateChanges();
			GraphicsDevice.GLDevice.ApplyEffect(
				glEffect,
				pass,
				ref stateChanges
			);
			// FIXME: Reduce allocs here! -flibit
			/* FIXME: Does this actually affect the XNA variables?
			 * There's a chance that the D3DXEffect calls do this
			 * behind XNA's back, even.
			 * -flibit
			 */
			if (stateChanges.render_state_change_count > 0)
			{
				bool blendStateChanged = false;
				bool depthStencilStateChanged = false;
				bool rasterizerStateChanged = false;
				BlendState newBlend = new BlendState()
				{
					AlphaBlendFunction = GraphicsDevice.BlendState.AlphaBlendFunction,
					AlphaDestinationBlend = GraphicsDevice.BlendState.AlphaDestinationBlend,
					AlphaSourceBlend = GraphicsDevice.BlendState.AlphaSourceBlend,
					ColorBlendFunction = GraphicsDevice.BlendState.ColorBlendFunction,
					ColorDestinationBlend = GraphicsDevice.BlendState.ColorDestinationBlend,
					ColorSourceBlend = GraphicsDevice.BlendState.ColorSourceBlend,
					ColorWriteChannels1 = GraphicsDevice.BlendState.ColorWriteChannels1,
					ColorWriteChannels2 = GraphicsDevice.BlendState.ColorWriteChannels2,
					ColorWriteChannels3 = GraphicsDevice.BlendState.ColorWriteChannels3,
					BlendFactor = GraphicsDevice.BlendState.BlendFactor,
					MultiSampleMask = GraphicsDevice.BlendState.MultiSampleMask
				};
				DepthStencilState newDepthStencil = new DepthStencilState()
				{
					DepthBufferEnable = GraphicsDevice.DepthStencilState.DepthBufferEnable,
					DepthBufferWriteEnable = GraphicsDevice.DepthStencilState.DepthBufferWriteEnable,
					DepthBufferFunction = GraphicsDevice.DepthStencilState.DepthBufferFunction,
					StencilEnable = GraphicsDevice.DepthStencilState.StencilEnable,
					StencilFunction = GraphicsDevice.DepthStencilState.StencilFunction,
					StencilPass = GraphicsDevice.DepthStencilState.StencilPass,
					StencilFail = GraphicsDevice.DepthStencilState.StencilFail,
					StencilDepthBufferFail = GraphicsDevice.DepthStencilState.StencilDepthBufferFail,
					TwoSidedStencilMode = GraphicsDevice.DepthStencilState.TwoSidedStencilMode,
					CounterClockwiseStencilFunction = GraphicsDevice.DepthStencilState.CounterClockwiseStencilFunction,
					CounterClockwiseStencilFail = GraphicsDevice.DepthStencilState.CounterClockwiseStencilFail,
					CounterClockwiseStencilPass = GraphicsDevice.DepthStencilState.CounterClockwiseStencilPass,
					CounterClockwiseStencilDepthBufferFail = GraphicsDevice.DepthStencilState.CounterClockwiseStencilDepthBufferFail,
					StencilMask = GraphicsDevice.DepthStencilState.StencilMask,
					StencilWriteMask = GraphicsDevice.DepthStencilState.StencilWriteMask,
					ReferenceStencil = GraphicsDevice.DepthStencilState.ReferenceStencil
				};
				RasterizerState newRasterizer = new RasterizerState()
				{
					CullMode = GraphicsDevice.RasterizerState.CullMode,
					FillMode = GraphicsDevice.RasterizerState.FillMode,
					DepthBias = GraphicsDevice.RasterizerState.DepthBias,
					MultiSampleAntiAlias = GraphicsDevice.RasterizerState.MultiSampleAntiAlias,
					ScissorTestEnable = GraphicsDevice.RasterizerState.ScissorTestEnable,
					SlopeScaleDepthBias = GraphicsDevice.RasterizerState.SlopeScaleDepthBias
				};
				MojoShader.MOJOSHADER_effectState* states = (MojoShader.MOJOSHADER_effectState*) stateChanges.render_state_changes;
				for (int i = 0; i < stateChanges.render_state_change_count; i += 1)
				{
					MojoShader.MOJOSHADER_renderStateType type = states[i].type;
					if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_ZENABLE)
					{
						MojoShader.MOJOSHADER_zBufferType* val = (MojoShader.MOJOSHADER_zBufferType*) states[i].value.values;
						newDepthStencil.DepthBufferEnable =
							(*val == MojoShader.MOJOSHADER_zBufferType.MOJOSHADER_ZB_TRUE) ?
								true : false;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_FILLMODE)
					{
						MojoShader.MOJOSHADER_fillMode* val = (MojoShader.MOJOSHADER_fillMode*) states[i].value.values;
						if (*val == MojoShader.MOJOSHADER_fillMode.MOJOSHADER_FILL_SOLID)
						{
							newRasterizer.FillMode = FillMode.Solid;
						}
						else if (*val == MojoShader.MOJOSHADER_fillMode.MOJOSHADER_FILL_WIREFRAME)
						{
							newRasterizer.FillMode = FillMode.WireFrame;
						}
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_ZWRITEENABLE)
					{
						int* val = (int*) states[i].value.values;
						newDepthStencil.DepthBufferWriteEnable = (*val == 1) ? true : false;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLEND)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						newBlend.ColorSourceBlend = XNABlend[*val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLEND)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						newBlend.ColorDestinationBlend = XNABlend[*val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CULLMODE)
					{
						MojoShader.MOJOSHADER_cullMode* val = (MojoShader.MOJOSHADER_cullMode*) states[i].value.values;
						if (*val == MojoShader.MOJOSHADER_cullMode.MOJOSHADER_CULL_NONE)
						{
							newRasterizer.CullMode = CullMode.None;
						}
						else if (*val == MojoShader.MOJOSHADER_cullMode.MOJOSHADER_CULL_CW)
						{
							newRasterizer.CullMode = CullMode.CullClockwiseFace;
						}
						else if (*val == MojoShader.MOJOSHADER_cullMode.MOJOSHADER_CULL_CCW)
						{
							newRasterizer.CullMode = CullMode.CullCounterClockwiseFace;
						}
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_ZFUNC)
					{
						MojoShader.MOJOSHADER_compareFunc* val = (MojoShader.MOJOSHADER_compareFunc*) states[i].value.values;
						newDepthStencil.DepthBufferFunction = XNACompare[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILENABLE)
					{
						int* val = (int*) states[i].value.values;
						newDepthStencil.StencilEnable = (*val == 1) ? true : false;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						newDepthStencil.StencilFail = XNAStencilOp[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILZFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						newDepthStencil.StencilDepthBufferFail = XNAStencilOp[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILPASS)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						newDepthStencil.StencilPass = XNAStencilOp[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILFUNC)
					{
						MojoShader.MOJOSHADER_compareFunc* val = (MojoShader.MOJOSHADER_compareFunc*) states[i].value.values;
						newDepthStencil.StencilFunction = XNACompare[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILREF)
					{
						int* val = (int*) states[i].value.values;
						newDepthStencil.ReferenceStencil = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILMASK)
					{
						int* val = (int*) states[i].value.values;
						newDepthStencil.StencilMask = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_STENCILWRITEMASK)
					{
						int* val = (int*) states[i].value.values;
						newDepthStencil.StencilWriteMask = *val;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEANTIALIAS)
					{
						int* val = (int*) states[i].value.values;
						newRasterizer.MultiSampleAntiAlias = (*val == 1) ? true : false;
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_MULTISAMPLEMASK)
					{
						int* val = (int*) states[i].value.values;
						newBlend.MultiSampleMask = *val;
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE)
					{
						int* val = (int*) states[i].value.values;
						newBlend.ColorWriteChannels = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOP)
					{
						MojoShader.MOJOSHADER_blendOp* val = (MojoShader.MOJOSHADER_blendOp*) states[i].value.values;
						newBlend.ColorBlendFunction = XNABlendOp[*val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SCISSORTESTENABLE)
					{
						int* val = (int*) states[i].value.values;
						newRasterizer.ScissorTestEnable = (*val == 1) ? true : false;
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SLOPESCALEDEPTHBIAS)
					{
						float* val = (float*) states[i].value.values;
						newRasterizer.SlopeScaleDepthBias = *val;
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_TWOSIDEDSTENCILMODE)
					{
						int* val = (int*) states[i].value.values;
						newDepthStencil.TwoSidedStencilMode = (*val == 1) ? true : false;
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						newDepthStencil.CounterClockwiseStencilFail = XNAStencilOp[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILZFAIL)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						newDepthStencil.CounterClockwiseStencilDepthBufferFail = XNAStencilOp[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILPASS)
					{
						MojoShader.MOJOSHADER_stencilOp* val = (MojoShader.MOJOSHADER_stencilOp*) states[i].value.values;
						newDepthStencil.CounterClockwiseStencilPass = XNAStencilOp[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_CCW_STENCILFUNC)
					{
						MojoShader.MOJOSHADER_compareFunc* val = (MojoShader.MOJOSHADER_compareFunc*) states[i].value.values;
						newDepthStencil.CounterClockwiseStencilFunction = XNACompare[*val];
						depthStencilStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE1)
					{
						int* val = (int*) states[i].value.values;
						newBlend.ColorWriteChannels1 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE2)
					{
						int* val = (int*) states[i].value.values;
						newBlend.ColorWriteChannels2 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_COLORWRITEENABLE3)
					{
						int* val = (int*) states[i].value.values;
						newBlend.ColorWriteChannels3 = (ColorWriteChannels) (*val);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDFACTOR)
					{
						// FIXME: RGBA? -flibit
						int* val = (int*) states[i].value.values;
						newBlend.BlendFactor = new Color(
							(*val >> 24) & 0xFF,
							(*val >> 16) & 0xFF,
							(*val >> 8) & 0xFF,
							*val & 0xFF
						);
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_DEPTHBIAS)
					{
						float* val = (float*) states[i].value.values;
						newRasterizer.DepthBias = *val;
						rasterizerStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_SRCBLENDALPHA)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						newBlend.AlphaSourceBlend = XNABlend[*val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_DESTBLENDALPHA)
					{
						MojoShader.MOJOSHADER_blendMode* val = (MojoShader.MOJOSHADER_blendMode*) states[i].value.values;
						newBlend.AlphaDestinationBlend = XNABlend[*val];
						blendStateChanged = true;
					}
					else if (type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_BLENDOPALPHA)
					{
						MojoShader.MOJOSHADER_blendOp* val = (MojoShader.MOJOSHADER_blendOp*) states[i].value.values;
						newBlend.AlphaBlendFunction = XNABlendOp[*val];
						blendStateChanged = true;
					}
					else if (	type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_VERTEXSHADER ||
							type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_PIXELSHADER	)
					{
						// Skip shader states
						continue;
					}
					else
					{
						// FIXME: ALPHABLENDENABLE -flibit
						// FIXME: SEPARATEALPHABLEND -flibit
						throw new Exception("Unhandled render state!");
					}
				}
				if (blendStateChanged)
				{
					GraphicsDevice.BlendState = newBlend;
				}
				if (depthStencilStateChanged)
				{
					GraphicsDevice.DepthStencilState = newDepthStencil;
				}
				if (rasterizerStateChanged)
				{
					GraphicsDevice.RasterizerState = newRasterizer;
				}
			}
			if (stateChanges.sampler_state_change_count > 0)
			{
				SamplerState[] samplers = new SamplerState[stateChanges.sampler_state_change_count];
				bool[] samplerChanged = new bool[stateChanges.sampler_state_change_count];
				for (int i = 0; i < samplers.Length; i += 1)
				{
					samplers[i] = new SamplerState()
					{
						Filter = GraphicsDevice.SamplerStates[i].Filter,
						AddressU = GraphicsDevice.SamplerStates[i].AddressU,
						AddressV = GraphicsDevice.SamplerStates[i].AddressV,
						AddressW = GraphicsDevice.SamplerStates[i].AddressW,
						MaxAnisotropy = GraphicsDevice.SamplerStates[i].MaxAnisotropy,
						MaxMipLevel = GraphicsDevice.SamplerStates[i].MaxMipLevel,
						MipMapLevelOfDetailBias = GraphicsDevice.SamplerStates[i].MipMapLevelOfDetailBias,
					};
				}
				MojoShader.MOJOSHADER_samplerStateRegister* registers = (MojoShader.MOJOSHADER_samplerStateRegister*) stateChanges.sampler_state_changes;
				for (int i = 0; i < stateChanges.sampler_state_change_count; i += 1)
				{
					if (registers[i].sampler_state_count == 0)
					{
						// Nothing to do
						continue;
					}
					int register = (int) registers[i].sampler_register;
					MojoShader.MOJOSHADER_effectSamplerState* states = (MojoShader.MOJOSHADER_effectSamplerState*) registers[i].sampler_states;
					for (int j = 0; j < registers[i].sampler_state_count; j += 1)
					{
						MojoShader.MOJOSHADER_samplerStateType type = states[j].type;
						if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_TEXTURE)
						{
							string textureName = Marshal.PtrToStringAnsi(
								registers[i].sampler_name
							);
							GraphicsDevice.Textures[register] = samplerMap[textureName].texture;
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSU)
						{
							MojoShader.MOJOSHADER_textureAddress* val = (MojoShader.MOJOSHADER_textureAddress*) states[j].value.values;
							samplers[register].AddressU = XNAAddress[*val];
							samplerChanged[register] = true;
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSV)
						{
							MojoShader.MOJOSHADER_textureAddress* val = (MojoShader.MOJOSHADER_textureAddress*) states[j].value.values;
							samplers[register].AddressV = XNAAddress[*val];
							samplerChanged[register] = true;
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_ADDRESSW)
						{
							MojoShader.MOJOSHADER_textureAddress* val = (MojoShader.MOJOSHADER_textureAddress*) states[j].value.values;
							samplers[register].AddressW = XNAAddress[*val];
							samplerChanged[register] = true;
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAGFILTER)
						{
							// FIXME: TextureFilter combinations -flibit
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MINFILTER)
						{
							// FIXME: TextureFilter combinations -flibit
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPFILTER)
						{
							// FIXME: TextureFilter combinations -flibit
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MIPMAPLODBIAS)
						{
							float* val = (float*) states[i].value.values;
							samplers[register].MipMapLevelOfDetailBias = *val;
							samplerChanged[register] = true;
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXMIPLEVEL)
						{
							int* val = (int*) states[i].value.values;
							samplers[register].MaxMipLevel = *val;
							samplerChanged[register] = true;
						}
						else if (type == MojoShader.MOJOSHADER_samplerStateType.MOJOSHADER_SAMP_MAXANISOTROPY)
						{
							int* val = (int*) states[i].value.values;
							samplers[register].MaxAnisotropy = *val;
							samplerChanged[register] = true;
						}
						else
						{
							throw new Exception("Unhandled sampler state!");
						}
					}
				}
				for (int i = 0; i < samplers.Length; i += 1)
				{
					if (samplerChanged[i])
					{
						GraphicsDevice.SamplerStates[i] = samplers[i];
					}
				}
			}
		}

		#endregion

		#region Private Methods

		private unsafe void INTERNAL_parseEffectStruct()
		{
			MojoShader.MOJOSHADER_effect* effectPtr = (MojoShader.MOJOSHADER_effect*) glEffect.EffectData;

			// Set up Parameters
			MojoShader.MOJOSHADER_effectParam* paramPtr = (MojoShader.MOJOSHADER_effectParam*) effectPtr->parameters;
			List<EffectParameter> parameters = new List<EffectParameter>();
			for (int i = 0; i < effectPtr->param_count; i += 1)
			{
				MojoShader.MOJOSHADER_effectParam param = paramPtr[i];
				if (	param.value.value_type == MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_VERTEXSHADER ||
					param.value.value_type == MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_PIXELSHADER	)
				{
					// Skip shader objects...
					continue;
				}
				else if (	param.value.value_type >= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLER &&
						param.value.value_type <= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLERCUBE	)
				{
					string textureName = String.Empty;
					MojoShader.MOJOSHADER_effectSamplerState* states = (MojoShader.MOJOSHADER_effectSamplerState*) param.value.values;
					for (int j = 0; j < param.value.value_count; j += 1)
					{
						if (	states[j].value.value_type >= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURE &&
							states[j].value.value_type <= MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_TEXTURECUBE	)
						{
							MojoShader.MOJOSHADER_effectObject *objectPtr = (MojoShader.MOJOSHADER_effectObject*) effectPtr->objects;
							int* index = (int*) states[j].value.values;
							textureName = Marshal.PtrToStringAnsi(objectPtr[*index].mapping.name);
							break;
						}
					}
					/* Because textures have to be declared before the sampler,
					 * we can assume that it will always be in the list by the
					 * time we get to this point.
					 * -flibit
					 */
					for (int j = 0; j < parameters.Count; j += 1)
					{
						if (textureName.Equals(parameters[j].Name))
						{
							samplerMap[Marshal.PtrToStringAnsi(param.value.name)] = parameters[j];
							break;
						}
					}
					continue;
				}
				parameters.Add(new EffectParameter(
					Marshal.PtrToStringAnsi(param.value.name),
					Marshal.PtrToStringAnsi(param.value.semantic),
					(int) param.value.row_count,
					(int) param.value.column_count,
					(int) param.value.element_count,
					XNAClass[param.value.value_class],
					XNAType[param.value.value_type],
					null, // FIXME: See mojoshader_effects.c:readvalue -flibit
					INTERNAL_readAnnotations(
						param.annotations,
						param.annotation_count
					),
					param.value.values
				));
			}
			Parameters = new EffectParameterCollection(parameters.ToArray());

			// Set up Techniques
			MojoShader.MOJOSHADER_effectTechnique* techPtr = (MojoShader.MOJOSHADER_effectTechnique*) effectPtr->techniques;
			EffectTechnique[] techniques = new EffectTechnique[effectPtr->technique_count];
			for (int i = 0; i < techniques.Length; i += 1)
			{
				MojoShader.MOJOSHADER_effectTechnique tech = techPtr[i];

				// Set up Passes
				MojoShader.MOJOSHADER_effectPass* passPtr = (MojoShader.MOJOSHADER_effectPass*) tech.passes;
				EffectPass[] passes = new EffectPass[tech.pass_count];
				for (int j = 0; j < passes.Length; j += 1)
				{
					MojoShader.MOJOSHADER_effectPass pass = passPtr[j];
					passes[j] = new EffectPass(
						Marshal.PtrToStringAnsi(pass.name),
						INTERNAL_readAnnotations(
							pass.annotations,
							pass.annotation_count
						),
						this,
						(uint) j
					);
				}

				techniques[i] = new EffectTechnique(
					Marshal.PtrToStringAnsi(tech.name),
					(IntPtr) (techPtr + i),
					new EffectPassCollection(passes),
					INTERNAL_readAnnotations(
						tech.annotations,
						tech.annotation_count
					)
				);
			}
			Techniques = new EffectTechniqueCollection(techniques);
		}

		private unsafe EffectAnnotationCollection INTERNAL_readAnnotations(
			IntPtr rawAnnotations,
			uint numAnnotations
		) {
			MojoShader.MOJOSHADER_effectAnnotation* annoPtr = (MojoShader.MOJOSHADER_effectAnnotation*) rawAnnotations;
			EffectAnnotation[] annotations = new EffectAnnotation[numAnnotations];
			for (int i = 0; i < numAnnotations; i += 1)
			{
				MojoShader.MOJOSHADER_effectAnnotation anno = annoPtr[i];
				annotations[i] = new EffectAnnotation(
					Marshal.PtrToStringAnsi(anno.name),
					Marshal.PtrToStringAnsi(anno.semantic),
					(int) anno.row_count,
					(int) anno.column_count,
					XNAClass[anno.value_class],
					XNAType[anno.value_type],
					anno.values
				);
			}
			return new EffectAnnotationCollection(annotations);
		}

		#endregion
	}
}
