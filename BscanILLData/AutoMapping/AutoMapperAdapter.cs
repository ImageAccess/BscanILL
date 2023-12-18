using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace BscanILLData.AutoMapping
{
	public class AutoMapperAdapter : IAutoMapper
	{
		public T Map<T>(object objectToMap)
		{
			return Mapper.Map<T>(objectToMap);
		}
	}
}
