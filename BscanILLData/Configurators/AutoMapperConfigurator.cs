using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BscanILLData.AutoMapping;

namespace BscanILLData.Configurators
{
	[SmartAssembly.Attributes.DoNotPruneType()]
	[SmartAssembly.Attributes.DoNotObfuscateType()]
	public class AutoMapperConfigurator
	{

		public void Configure(IEnumerable<BscanILLData.Configurators.IAutoMapperConfigurator> autoMapperTypeConfigurations)
		{
			autoMapperTypeConfigurations.ToList().ForEach(x => x.Configure());

			Mapper.AssertConfigurationIsValid();
		}

	}
}
