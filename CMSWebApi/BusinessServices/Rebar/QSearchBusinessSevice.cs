using CMSWebApi.DataModels;
using CMSWebApi.DataModels.ModelBinderProvider;
using CMSWebApi.ServiceInterfaces;
using CMSWebApi.Utils;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using CMSWebApi.DataServices;
using Extensions.Linq;
using System.Linq.Expressions;
using System.Collections;
using LinqKit;

namespace CMSWebApi.BusinessServices.Rebar
{
	public class QSearchBusinessSevice : BusinessBase<IQuickSearchService>
	{
		#region Properties
		const string linq_datetime_format = "yyyy-MM-dd HH:mm:ss";
		
		public IUsersService IUser { get; set; }
		public ICompanyService comSvc { get; set; }
		public ISiteService ISiteSvc { get; set; }
		public IRebarDataService IRebar { get; set; }

		#endregion

		public IQueryable<QSSummaryModel> QuickSearchReport(UserContext userLogin, QuickSearchParam param)
		{
			
			ConvertParams(ref param, userLogin);

			IQueryable<tbl_POS_Transact> transact =  BuiltTransactLinqKitQuery(param);
			try
			{
			IQueryable<QSSummaryModel> result;
			if (param.DescIDs != null && param.DescIDs.Any() && param.DescIDs_AND == true)
				{
					result = transact.Select<tbl_POS_Transact,QSSummaryModel>(trans => new QSSummaryModel
																{
																	TransID = trans.TransID,
																	PACID = trans.T_PACID,
																	T_OperatorID = trans.T_OperatorID,
																	T_6TotalAmount = trans.T_6TotalAmount,
																	DVRDate = trans.DVRDate,
																	Payments = trans.tbl_POS_TransPayment.Select( p=> new Payment{ PaymentID = p.PaymentID, PaymentAmount = p.PaymentAmount}),
																	//T_00TransNBText = trans.T_00TransNBText,
																	T_0TransNB = trans.T_0TransNB,
																	T_1SubTotal = trans.T_1SubTotal,
																	T_8ChangeAmount = trans.T_8ChangeAmount,
																	T_9RecItemCount = trans.T_9RecItemCount,
																	T_CameraNB = trans.T_CameraNB,
																	T_CardID = trans.T_CardID,
																	T_CheckID = trans.T_CheckID,
																	T_RegisterID = trans.T_RegisterID,
																	T_ShiftID = trans.T_ShiftID,
																	T_StoreID = trans.T_StoreID,
																	T_TerminalID = trans.T_TerminalID,
																	Taxes = trans.tbl_POS_TransTaxes.Select( t => new Taxes{ TaxID = t.TaxID, TaxAmount = t.TaxAmount}),
																	TransDate = trans.TransDate,
																	Description = trans.tbl_POS_Retail.Join( param.DescIDs, r => r.R_Description, it=> it, (l,r)=> new RetailSummary{ Description = l.R_Description, R_0Amount = l.R_0Amount})
																});
				}
				else{

					result = transact.Select<tbl_POS_Transact, QSSummaryModel>(trans => new QSSummaryModel
																						{
																							TransID = trans.TransID,
																							PACID = trans.T_PACID,
																							T_OperatorID = trans.T_OperatorID,
																							T_6TotalAmount = trans.T_6TotalAmount,
																							DVRDate = trans.DVRDate,
																							Payments = trans.tbl_POS_TransPayment.Select(p => new Payment { PaymentID = p.PaymentID, PaymentAmount = p.PaymentAmount }),
																							//T_00TransNBText = trans.T_00TransNBText,
																							T_0TransNB = trans.T_0TransNB,
																							T_1SubTotal = trans.T_1SubTotal,
																							T_8ChangeAmount = trans.T_8ChangeAmount,
																							T_9RecItemCount = trans.T_9RecItemCount,
																							T_CameraNB = trans.T_CameraNB,
																							T_CardID = trans.T_CardID,
																							T_CheckID = trans.T_CheckID,
																							T_RegisterID = trans.T_RegisterID,
																							T_ShiftID = trans.T_ShiftID,
																							T_StoreID = trans.T_StoreID,
																							T_TerminalID = trans.T_TerminalID,
																							Taxes = trans.tbl_POS_TransTaxes.Select(t => new Taxes { TaxID = t.TaxID, TaxAmount = t.TaxAmount }),
																							TransDate = trans.TransDate,
																							Description = trans.tbl_POS_Retail.Select(r => new RetailSummary{ Description = r.R_Description, R_0Amount = r.R_0Amount})
																						});
				}
				return result;
			}
			catch(Exception){
				return System.Linq.Enumerable.Empty<QSSummaryModel>().AsQueryable();
			}
			//var kit_ret1 = await kit1.ToListAsync();

			
			//var data = await DataService.QuickSearchReports(param);
			//var retData = data;
			//return retData;
		}

		private void ConvertParams(ref QuickSearchParam param, UserContext userLogin)
		{
			QuickSearchParam ret = param;
			ret.DateFrom = param.DateFrom;
			ret.DateTo = param.DateTo;
			if (param.PACIDs.Count == 0)
			{
				IEnumerable<UserSiteDvrChannel> userSiteDvrChannel = UserSites(IUser, userLogin).Where(w => ret.SiteKeys.Contains(w.siteKey.Value)).Distinct();
				ret.PACIDs = userSiteDvrChannel.Select(s => s.PACID.Value).Distinct().ToList();
			}
			else
			{
				ret.PACIDs = param.PACIDs;
			}


			param = ret;
		}

		#region Mark
		/*
		internal IQueryable<T> InnerjoinList<T, P>(IQueryable<T> inner, List<P> outer, Expression<Func<T,P>> property)
		{
			return inner.Join(outer, property, outkey => outkey, (ret, outkey)=> ret);
		}
		internal IQueryable<T> InnerjoinList<T>(IQueryable<T> inner, List<int> outer, Expression<Func<T, int>> property)
		{
			return inner.Join(outer, property, outkey => outkey, (ret, outkey) => ret);
		}
		internal IQueryable<T> InnerjoinList<T>(IQueryable<T> inner, List<int> outer, Expression<Func<T, int?>> property)
		{
			return inner.Join(outer, property, outkey => outkey, (ret, outkey) => ret);
		}

		internal void AndCompare( string field, string op,  object value, ref string filter)
		{
			if( string.IsNullOrEmpty(filter))
				filter = string.Format(" {0} {1} {2} ", field, op, value);
			else
				filter += string.Format(" And {0} {1} {2} ", field, op, value);
		}

		internal IQueryable<tbl_POS_Transact> GetTrans(List<int> pacids)
		{
			IServiceBase<tbl_POS_Transact> service = new DataServices.POS.TransactModelService(ServiceBase);
			IQueryable<tbl_POS_Transact> query = service.Gets(null, null);
			return query.Join(pacids, trans => trans.T_PACID, it => it, (Trans, it) => Trans); 
		}


		internal IQueryable<tbl_POS_Transact> BuiltRetailQuery(QuickSearchParam param)
		{
			//IServiceBase<tbl_POS_Retail> service_rtail = new DataServices.POS.RetailModelService(ServiceBase);
			IServiceBase<tbl_POS_Transact> service_trans = new DataServices.POS.TransactModelService(ServiceBase);
			IQueryable<tbl_POS_Transact> trans = service_trans.Gets( it => it.DVRDate >= param.DateFrom && it.DVRDate <= param.DateTo, null);
			var itrans = trans.Join( param.DescIDs, it=> it.T_PACID, p => p, (t,p)=> t);
			itrans.Include<tbl_POS_Transact, ICollection<tbl_POS_Retail>>( it => it.tbl_POS_Retail );
			return itrans.Where( it => it.tbl_POS_Retail.Any( r=> param.DescIDs.Any( p=> p == r.R_Description.Value)));
		}
		internal IQueryable<tbl_POS_Transact> BuiltTransactLinqKitQuery(QuickSearchParam param)
		{
			IServiceBase<tbl_POS_Transact> service = new DataServices.POS.TransactModelService(ServiceBase);
			IQueryable<tbl_POS_Transact> trans = service.Gets(null);
			var root = PredicateBuilder.True<tbl_POS_Transact>();
			root = root.And(it => it.DVRDate >= param.DateFrom);
			root = root.And(it => it.DVRDate <= param.DateTo);
			root = root.And(it => param.PACIDs.Contains(it.T_PACID));
			
			var and_comapre = PredicateBuilder.True<tbl_POS_Transact>();
			var or_compare = PredicateBuilder.False<tbl_POS_Transact>();

			Expression<Func<tbl_POS_Transact, bool>> trans_comapre = null;

			if (param.EmpIDs.Any())
			{	
				trans_comapre = it =>  it.T_OperatorID.HasValue && param.EmpIDs.Contains( it.T_OperatorID.Value);
				if( param.EmpIDs_AND)
					and_comapre = and_comapre.And(trans_comapre);
					
				else
					or_compare = or_compare.Or(trans_comapre);
					
			}

			if (param.RegIDs != null && param.RegIDs.Any())
			{
				trans_comapre = it => param.RegIDs.Contains(it.T_RegisterID.Value);
				if (param.RegIDs_AND)
					
					and_comapre = and_comapre.And(trans_comapre);
				else
					or_compare = or_compare.Or(trans_comapre);
					
			}

			
			if (string.Compare(param.TransNB_OP, "Any", true) != 0)
			{
				
				switch (param.TransNB_OP)
				{
					case "=":
						trans_comapre = it => it.T_0TransNB == (long?)param.TransNB;
						break;
					case ">":
						trans_comapre = it => it.T_0TransNB > (long?)param.TransNB;
						break;
					case "<":
						trans_comapre = it => it.T_0TransNB < (long?)param.TransNB;
						break;
				}

				if (param.TransNB_AND == true)
					
					and_comapre = and_comapre.And(trans_comapre);
				else
					or_compare = or_compare.Or(trans_comapre);
					
			}

			if (string.Compare(param.TransAmount_OP, "Any", true) != 0)
			{
				switch (param.TransAmount_OP)
				{
					case "=":
						trans_comapre = it => it.T_6TotalAmount == (decimal?)param.TransAmount;
						break;
					case ">":
						trans_comapre = it => it.T_6TotalAmount > (decimal?)param.TransAmount;
						break;
					case "<":
						trans_comapre =  it => it.T_6TotalAmount <(decimal?)param.TransAmount;
						break;
				}

				if (param.TransAmount_AND == true)
					
					and_comapre = and_comapre.And(trans_comapre);

				else
					or_compare = or_compare.Or(trans_comapre);
					

			}

			if (param.DescIDs != null && param.DescIDs.Any())
			{

				trans = trans.Include<tbl_POS_Transact, ICollection<tbl_POS_Retail>>(it => it.tbl_POS_Retail);
				trans_comapre = it => it.tbl_POS_Retail.Any( r => param.DescIDs.Contains(r.R_Description.Value));
				if( param.DescIDs_AND)
					
					and_comapre = and_comapre.And(trans_comapre);
				else
					or_compare = or_compare.Or(trans_comapre);
					
			}
			var combine = PredicateBuilder.True<tbl_POS_Transact>();
			combine = combine.And( and_comapre);
			combine = combine.Or( or_compare);

			var result = trans.Where(root).AsExpandable().Where(combine.Expand());
			return result;
		}
		*/
		#endregion

		internal IQueryable<tbl_POS_Transact> BuiltTransactLinqKitQuery(QuickSearchParam param)
		{
			InternalBusinessService.POSBusinessService bo = new InternalBusinessService.POSBusinessService(ServiceBase, param.DateFrom, param.DateTo, param.PACIDs);
			Expression<Func<tbl_POS_Transact, bool>> trans_comapre = null;

			if (param.EmpIDs.Any())
			{
				trans_comapre = it => it.T_OperatorID.HasValue && param.EmpIDs.Contains(it.T_OperatorID.Value);
				if (param.EmpIDs_AND)
					bo.And(trans_comapre);

				else
					bo.Or(trans_comapre);
			}

			if(param.PaymentIDs != null && param.PaymentIDs.Any())
			{
				trans_comapre = it => it.tbl_POS_TransPayment.Any( p => param.PaymentIDs.Contains( p.PaymentID));
				if(param.PaymentIDs_AND == true)
					bo.And( trans_comapre);
				else
					bo.Or(trans_comapre);
					
			}
			if (param.RegIDs != null && param.RegIDs.Any())
			{
				trans_comapre = it => param.RegIDs.Contains(it.T_RegisterID.Value);
				if (param.RegIDs_AND)
					bo.And(trans_comapre);
				else
					bo.Or(trans_comapre);

			}


			if (string.Compare(param.TransNB_OP, "Any", true) != 0)
			{
				trans_comapre = Compare<long?>(param.TransNB_OP, (long?)param.TransNB, it => it.T_0TransNB);
				if (param.TransNB_AND == true)
					bo.And(trans_comapre);
				else
					bo.Or(trans_comapre);

			}

			if (string.Compare(param.TransAmount_OP, "Any", true) != 0)
			{
				trans_comapre = Compare<decimal?>(param.TransAmount_OP, (decimal?)param.TransAmount, it => it.T_6TotalAmount);
				if (param.TransAmount_AND == true)
					bo.And(trans_comapre);
				else
					bo.Or(trans_comapre);
			}

			if (param.DescIDs != null && param.DescIDs.Any())
			{
				
				trans_comapre = it => it.tbl_POS_Retail.Any(r => param.DescIDs.Contains(r.R_Description.Value));
				if (param.DescIDs_AND)
					bo.And(trans_comapre);
				else
					bo.Or(trans_comapre);

			}
			bo.Include<tbl_POS_Retail>(it => it.tbl_POS_Retail);
			bo.Include<tbl_POS_TransPayment>(it => it.tbl_POS_TransPayment);
			bo.Include<tbl_POS_TransTaxes>(it => it.tbl_POS_TransTaxes);
			return bo.Result<tbl_POS_Transact>(it => it);
		}

		internal Expression<Func<tbl_POS_Transact, bool>> Compare<T>(string op, T value, Expression<Func<tbl_POS_Transact, T>> selector)
		{
			Expression<Func<tbl_POS_Transact, bool>> _comapre = null;

			var parameter = selector.Parameters [0];
			var left = selector.Body;
			var right = Expression.Constant(value, typeof(T));
			BinaryExpression binaryExpression = null;
			switch (op)
			{
				case "=":
					binaryExpression = Expression.Equal(left, right);
					break;
				case ">":
					binaryExpression = Expression.GreaterThan(left, right);
					break;
				case "<":
					binaryExpression = Expression.LessThan(left, right);
					break;
				case ">=":
					binaryExpression = Expression.GreaterThanOrEqual(left, right);
					break;
				case "<=":
					binaryExpression = Expression.LessThanOrEqual(left, right);
					break;
			}
			_comapre = Expression.Lambda<Func<tbl_POS_Transact, bool>>(binaryExpression, parameter);
			return _comapre;
		}
		
		public IQueryable<TransactionDetailModel>TransactionDetail(IEnumerable<long> tranIDs)
		{
			if( tranIDs == null || tranIDs.Any() == false)
				return System.Linq.Enumerable.Empty<TransactionDetailModel>().AsQueryable();

			IQueryable<TransactionDetailModel> result = null;

			InternalBusinessService.POSBusinessService bo = new InternalBusinessService.POSBusinessService(ServiceBase, null,  null,  null);
			bo.Include< tbl_POS_TransTaxes>( it=> it.tbl_POS_TransTaxes);
			bo.Include<tbl_POS_Retail>(it => it.tbl_POS_Retail);
			bo.Include<tbl_POS_TransExtraNumber>(it => it.tbl_POS_TransExtraNumber);
			bo.Include<tbl_POS_TransExtraString>(it => it.tbl_POS_TransExtraString);
			bo.Include<tbl_POS_TransPayment>(it => it.tbl_POS_TransPayment);

			IEnumerable<KeyValuePair<string, Type>> path = ServiceBase.ChildPropertyMaps(typeof(tbl_POS_Retail));

			bo.Include("tbl_POS_Retail.tbl_POS_RetailExtraString");
			bo.Include("tbl_POS_Retail.tbl_POS_RetailExtraNumber");

			bo.AndRoot( it => tranIDs.Contains( it.TransID));
			result = bo.Result<TransactionDetailModel>(ToTransactModel);
			return result;
		}

		static readonly internal Expression<Func<tbl_POS_Transact, TransactionDetailModel>> ToTransactModel =

			it => new TransactionDetailModel
			{
				DVRDate = it.DVRDate,
				ExtraNumber = it.tbl_POS_TransExtraNumber.Select<tbl_POS_TransExtraNumber, ExtraNumber>(texn => new ExtraNumber { ExtraID = texn.ExtraID, ExNum_Value = texn.ExNum_Value }),
				ExtraString = it.tbl_POS_TransExtraString.Select<tbl_POS_TransExtraString, ExtraString>(texs => new ExtraString { ExtraID = texs.ExtraID, ExString_ValueID = texs.ExString_ValueID }),
				Payments = it.tbl_POS_TransPayment.Select<tbl_POS_TransPayment, Payment>(pay => new Payment { PaymentID = pay.PaymentID, PaymentAmount = pay.PaymentAmount }),
				Retails = it.tbl_POS_Retail.Select<tbl_POS_Retail, RetailModel>(rt => new RetailModel
																						{
																							R_0Amount = rt.R_0Amount,
																							R_1Qty = rt.R_1Qty,
																							R_2ItemLineNb = rt.R_2ItemLineNb,
																							R_Description = rt.R_Description,
																							R_DVRDate = rt.R_DVRDate,
																							R_ItemCode = rt.R_ItemCode,
																							RetailID = rt.RetailID,
																							//SubRetail = r.tbl_POS_SubRetail.Select<tbl_POS_SubRetail, SubRetailModel>(ToSubretailModel) ,
																							ExtraNumber = rt.tbl_POS_RetailExtraNumber.Select<tbl_POS_RetailExtraNumber, ExtraNumber>(rexn => new ExtraNumber { ExtraID = rexn.ExtraID, ExNum_Value = rexn.ExNum_Value }),
																							ExtraString = rt.tbl_POS_RetailExtraString.Select<tbl_POS_RetailExtraString, ExtraString>(rexs => new ExtraString { ExtraID = rexs.ExtraID, ExString_ValueID = rexs.ExString_ValueID })
																						}),
				T_00TransNBText = it.T_00TransNBText,
				T_0TransNB = it.T_0TransNB,
				T_1SubTotal = it.T_1SubTotal,
				T_6TotalAmount = it.T_6TotalAmount,
				T_8ChangeAmount = it.T_8ChangeAmount,
				T_9RecItemCount = it.T_9RecItemCount,
				T_CameraNB = it.T_CameraNB,
				T_CardID = it.T_CardID,
				T_CheckID = it.T_CheckID,
				T_OperatorID = it.T_OperatorID,
				T_PACID = it.T_PACID,
				T_RegisterID = it.T_RegisterID,
				T_ShiftID = it.T_ShiftID,
				T_StoreID = it.T_StoreID,
				T_TerminalID = it.T_TerminalID,
				Taxes = it.tbl_POS_TransTaxes.Select<tbl_POS_TransTaxes, Taxes>(tax => new Taxes { TaxID = tax.TaxID, TaxAmount = tax.TaxAmount }),
				TransDate = it.TransDate,
				TransID = it.TransID
			};
		
		static readonly internal Func<tbl_POS_Retail, RetailModel> ToRetailModel =
		
			r => new RetailModel
			{
				R_0Amount = r.R_0Amount,
				R_1Qty = r.R_1Qty,
				R_2ItemLineNb = r.R_2ItemLineNb,
				R_Description = r.R_Description,
				R_DVRDate = r.R_DVRDate,
				R_ItemCode = r.R_ItemCode,
				RetailID = r.RetailID,
				//SubRetail = r.tbl_POS_SubRetail.Select<tbl_POS_SubRetail, SubRetailModel>(ToSubretailModel) ,
				//ExtraNumber = r.tbl_POS_RetailExtraNumber.Select<tbl_POS_RetailExtraNumber, ExtraNumber>(rexn => new ExtraNumber { ExtraID = rexn.ExtraID, ExNum_Value = rexn.ExNum_Value }),
				//ExtraString = r.tbl_POS_RetailExtraString.Select<tbl_POS_RetailExtraString, ExtraString>(rexs => new ExtraString { ExtraID = rexs.ExtraID, ExString_ValueID = rexs.ExString_ValueID })
			};


		static readonly internal Func<tbl_POS_SubRetail,SubRetailModel> ToSubretailModel = sr => 
			new SubRetailModel
			{
				SubRetailID = sr.SubRetailID,
				SR_0Amount = sr.SR_0Amount,
				SR_1Qty = sr.SR_1Qty,
				SR_2SubItemLineNb = sr.SR_2SubItemLineNb,
				SR_Description = sr.SR_Description
			};
		
	}
 }