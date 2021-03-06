﻿using Flh.Business;
using Flh.Business.Data;
using Flh.Web;
using Flh.WebSite.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Flh.WebSite.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductManager _ProductManager;
        private readonly IClassesManager _ClassesManager;
        private readonly IProductServiceFactory _ProductServiceFactory;
        public ProductController(IProductManager productManager, 
            IClassesManager classesManager,
            IProductServiceFactory productServiceFactory)
        {
            _ProductManager = productManager;
            _ClassesManager = classesManager;
            _ProductServiceFactory= productServiceFactory;
        }

        public ActionResult Index(string no, string kw, int? page,SortType? sort,decimal? priceMin,decimal? priceMax,string color)
        {
            if (!page.HasValue || page.Value < 1)
            {
                page = 1;
            }
            var size = 12;
            var count = 0;

            //获取一级分类
            no=no??String.Empty;
            var classOneNo = no ?? String.Empty;
            var classTwoNo = String.Empty;
            if (no.Length < 8)
            {
                classOneNo = FlhConfig.CLASSNO_CLASS_PREFIX;
                classTwoNo=String.Empty;
            }
            else if (no.Length > 8)
            {
                classOneNo = no.Substring(0,8);
                classTwoNo = no.Left(12);
            }
            else if (no.Length == 8)
            {
                classOneNo = FlhConfig.CLASSNO_CLASS_PREFIX;
                classTwoNo=no;
            }

            var classOneName = String.Empty;
            var classTwoName = String.Empty;
            var classOne = _ClassesManager.EnabledClasses.FirstOrDefault(d => d.no == classOneNo);
            if (classOne != null)
            {
                if (classOneNo.Length <= 4)
                {
                    classOne.name = "所有分类";
                }
                classOneName = classOne.name;
            }
            

            List<ListModel.ClassItem> classes = new List<Models.Product.ListModel.ClassItem>();
            classes.Add(new ListModel.ClassItem { Name = classOne.name, No = classOne.no });

            var subClasses = _ClassesManager.GetChildren(classOneNo)
                .OrderByDescending(d => d.order_by)
                .Select(d => new Flh.WebSite.Models.Product.ListModel.ClassItem { Name = d.name, No = d.no }).ToArray();
            classes.AddRange(subClasses);

            if (!String.IsNullOrWhiteSpace(classTwoNo))
            {
                var classTwo = subClasses.FirstOrDefault(d=>d.No==classTwoNo);
                if (classTwo != null)
                {
                    classTwoName = classTwo.Name;
                }
            }

            var products = _ProductManager.Search(new ProductSearchArgs
            {
                Keyword = kw,
                ClassNo = String.IsNullOrWhiteSpace(no) ? String.Empty : no,
                PriceMin = priceMin,
                PriceMax = priceMax,
                Limit = size,
                Start = (page.Value - 1) * size,               
                Sort=sort,
                Color=color,
            }, out count);
            return View(new Models.Product.ListModel()
            {
                No = (no ?? String.Empty).Trim(),
                Keyword = (kw ?? String.Empty).Trim(),
                Color=color,
                ClassItems = classes.ToArray(),
                ClassOneNo=classOneNo,
                ClassOneName=classOneName,
                ClassTwoName=classTwoName,
                PriceMin=priceMin,
                PriceMax=priceMax,
                Sort=sort,
                Items = new PageModel<Models.Product.ListModel.Item>(products
                            .Select(p => new Models.Product.ListModel.Item
                            {
                                Pid = p.pid,
                                Name = p.name,
                                No = p.classNo,
                                Order = p.sortNo,
                                Color = p.color,
                                Image = p.imagePath,
                                Material = p.material,
                                Size = p.size,
                                Technique = p.technique
                            }), page.Value, (int)Math.Ceiling((double)count / (double)size))
            });
        }

        public ActionResult Detail(long id,long? rno)
        {
            //index = index < 0 ? new Nullable<int>() : index;

            var product = _ProductServiceFactory.CreateService(id);
            //var items = GetRelationProducts(id,null,product.Entity.classNo);
            product.AddViewCount();

            //面包线导航
            var levelSize=4;
            var levelCount = (product.Entity.classNo ?? String.Empty).Length / levelSize;
            var noes = new List<string>();
            for (var i = 1; i < levelCount; i++)
            {
                var no = product.Entity.classNo.Substring(0, i * levelSize);
                noes.Add(no);
            }
            var no_name_items =  _ClassesManager.EnabledClasses.Where(d => noes.Contains(d.no))
                .OrderByDescending(d => d.order_by).ThenByDescending(d => d.updated)
                .Select(d => new { no = d.no, name = d.name }).ToArray();
            StringBuilder sbNav = new StringBuilder();
            foreach (var item in no_name_items)
            {
                sbNav.Append("<a href='/Product?no=" + item.no + "'>" + item.name + "</a>&gt");
            }

            //上一批产品和下一批产品
            var TAKE_SIZE=7;
            if (!rno.HasValue)
            {
                var entity = _ProductManager.GetProjectsWithRowNumber(new ProductWithRowNoItemArgs { ClassNo = product.Entity.classNo, Pid = id }).FirstOrDefault();
                if (entity != null)
                {
                    rno = (int)entity.rno;
                }
                else
                {
                    rno = 0;
                }
            }
            var leftSize = (TAKE_SIZE - 1) / 2;
            long min = Math.Max( rno.Value - leftSize,1);
            var rithtSize = TAKE_SIZE-(rno.Value - min)-1;
            long max = rno.Value + rithtSize;
            var items = _ProductManager.GetProjectsWithRowNumber(new ProductWithRowNoItemArgs
            { 
                ClassNo=product.Entity.classNo,
                MinRno=min,
                MaxRno=max});
            return View(new ProductDetailModel { Detail = product.Entity, Items = items, BreadLine = sbNav.ToString(), IsLogin = base.CurrentUser != null });          
        }

        public ActionResult RelationProducts(long excludePid,long excludeMinPid,string no)
        {
            var items = GetRelationProducts(excludePid, excludeMinPid, no);
            return SuccessJsonResult(items);
        }

        Product[] GetRelationProducts(long excludePid, long? excludeMinPid, string no)
        {
            var query = _ProductManager.EnabledProducts.Where(d => d.pid != excludePid && d.classNo.StartsWith(no));
            if(excludeMinPid.HasValue){
                query=query.Where(d=>d.pid>excludeMinPid.Value);
            }
            return query.Take(30).ToArray();
        }
    }
   
    public class ProductDetailModel
    {
        public ProductDetailModel()
        {
            Items = new ProductWithRowNoItem[0];
        }
        public IProduct Detail { get; set; }
        public ProductWithRowNoItem[] Items { get; set; }
        public bool IsLogin { get; set; }
        public String BreadLine { get; set; }
    }
}
