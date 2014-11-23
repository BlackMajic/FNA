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
					if (	type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_VERTEXSHADER ||
						type == MojoShader.MOJOSHADER_renderStateType.MOJOSHADER_RS_PIXELSHADER	)
					{
						// Skip shader states
						continue;
					}
					System.Console.WriteLine("RS " + i.ToString() + ": " + type.ToString());
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
				MojoShader.MOJOSHADER_samplerStateRegister* states = (MojoShader.MOJOSHADER_samplerStateRegister*) stateChanges.sampler_state_changes;
				for (int i = 0; i < stateChanges.sampler_state_change_count; i += 1)
				{
					if (states[i].sampler_state_count == 0)
					{
						// Nothing to do
						continue;
					}
					System.Console.WriteLine("SS REGISTER: " + states[i].sampler_register.ToString());
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
