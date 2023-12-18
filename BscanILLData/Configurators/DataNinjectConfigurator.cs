using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BscanILLData.AutoMapping;
using BscanILLData.Helpers;
using NHibernate;
using Ninject;
using Ninject.Modules;
using Ninject.Extensions.Conventions;


namespace BscanILLData.Configurators
{
	public class DataNinjectConfigurator : NinjectModule
	{

		//PUBLIC METHODS
		#region public methods

		#region Load()
		public override void Load()
		{
			AddBindings();
			ConfigureAutoMapper();
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region AddBindings()
		private void AddBindings()
		{
			try
			{
				//Bind<IDateTime>().To<DateTimeAdapter>().InSingletonScope();

				//Bind<BscanILLData.QueryProcessors.SqlServer.IGetUsersProcessor>().To<BscanILLData.QueryProcessors.SqlServer.GetUsersProcessor>().InTransientScope();
			}
			catch (Exception ex)
			{
				throw new Exception("ERROR BscanILLData.Configurators.DataNinjectConfigurator.AddBindings(): " + ex.Message, ex);
			}
		}
		#endregion

		#region ConfigureAutoMapper()
		private void ConfigureAutoMapper()
		{
			try
			{
				var iAutoMapperTypeConfigurator = typeof(IAutoMapperConfigurator);

				var types = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(s => s.GetTypes())
					.Where(p => p.IsAbstract == false && iAutoMapperTypeConfigurator.IsAssignableFrom(p))
					.Select(x => (IAutoMapperConfigurator)Activator.CreateInstance(x));

				new AutoMapperConfigurator().Configure(types);

				Bind<IAutoMapper>().To<AutoMapperAdapter>().InSingletonScope();
			}
			catch (Exception ex)
			{
				throw new Exception("ERROR BscanILLData.Configurators.DataNinjectConfigurator.ConfigureAutoMapper(): " + ex.Message, ex);
			}
		}
		#endregion

		#endregion
	
	}
}
