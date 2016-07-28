using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using CMSWebApi.APIFilters._3rdToken;
using CMSWebApi.BusinessServices.Account;
using CMSWebApi.ServiceInterfaces;

namespace CMSWebApi._3rd
{
	[_3rdAuthenticationAttribute]
	public class OrdersController : ApiControllerBase<IAccountService, AccountsBusinessService>
	{
		
		public IHttpActionResult Get()
		{
			ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;

			var Name = ClaimsPrincipal.Current.Identity.Name;

			return new ApiActionResult<List<Order>>(System.Net.HttpStatusCode.OK, Order.CreateOrders(), base.Request); //Ok(Order.CreateOrders());
		}

		
		public IHttpActionResult Post(Order order)
		{
			return Ok(order);
		}
		public IHttpActionResult Put(int id, Order order)
		{
			return Ok(order);
		}
	}

	public class Order
	{
		public int OrderID { get; set; }
		public string CustomerName { get; set; }
		public string ShipperCity { get; set; }
		public Boolean IsShipped { get; set; }


		public static List<Order> CreateOrders()
		{
			List<Order> OrderList = new List<Order> 
            {
                new Order {OrderID = 10248, CustomerName = "Taiseer Joudeh", ShipperCity = "Amman", IsShipped = true },
                new Order {OrderID = 10249, CustomerName = "Ahmad Hasan", ShipperCity = "Dubai", IsShipped = false},
                new Order {OrderID = 10250,CustomerName = "Tamer Yaser", ShipperCity = "Jeddah", IsShipped = false },
                new Order {OrderID = 10251,CustomerName = "Lina Majed", ShipperCity = "Abu Dhabi", IsShipped = false},
                new Order {OrderID = 10252,CustomerName = "Yasmeen Rami", ShipperCity = "Kuwait", IsShipped = true}
            };

			return OrderList;
		}
	}
}
