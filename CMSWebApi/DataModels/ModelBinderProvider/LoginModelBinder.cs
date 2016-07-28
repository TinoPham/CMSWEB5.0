using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;

namespace CMSWebApi.DataModels.ModelBinderProvider
{
	public class LoginModelBinderProvider : System.Web.Http.ModelBinding.ModelBinderProvider
	{
		public override IModelBinder GetBinder(System.Web.Http.HttpConfiguration configuration, Type modelType)
		{
			return new LoginModelBinder();
		}
	}

	public class LoginModelBinder : IModelBinder
	{
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{

			if (bindingContext.ModelType != typeof(LoginModel))
			{
				return false;
			}
			string ct = actionContext.Request.Content.ReadAsStringAsync().Result;
			object ret = JsonConvert.DeserializeObject(ct, bindingContext.ModelType);
			bindingContext.Model = ret;

			return true;
		}

		private T TryGet<T>(ModelBindingContext bindingContext, string key) where T : class
		{
			if (String.IsNullOrEmpty(key))
				return null;

			ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + key);
			if (valueResult == null && bindingContext.FallbackToEmptyPrefix == true)
				valueResult = bindingContext.ValueProvider.GetValue(key);

			bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

			if (valueResult == null)
				return null;

			try
			{
				return valueResult.ConvertTo(typeof(T)) as T;
			}
			catch (Exception ex)
			{
				bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex);
				return null;
			}
		}
	}
}
