﻿using KKKKPPP.Data;
using KKKKPPP.Data.Models;
using KKKKPPP.Data.Interfaces;
using KKKKPPP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using System.Text;

namespace KKKKPPP.Controllers
{
    public class DBQueryController : Controller
    {
        private readonly IАвтор _Authors;
        private readonly IТехника _Techs;
        private readonly IСостояние _Conds;
        private readonly IСтатусКартины _StatsP;
        private readonly IСтрана _Countries;
        private readonly IЖанр _Janres;
        private readonly IСтиль _Styles;
        private readonly IСущности _Entities;
        private readonly IКартина _Pictures;
        private readonly IМатериал _Materials;
        private readonly IРеставрация _Restorations;
        private readonly IСвязь_Материал_Картина _Pic_Materials;
        private readonly IСвязь_Рест_Вид _Rest_Types;
        private readonly IВид_реставрации _RestorationTypes;
        private readonly IЭкспонат _Showp;
        private readonly IЭкспозиция _Expos;
        private readonly IЗал _Rooms;
        private readonly IМесто _Places;
        private readonly AppDBContext db;
        private string selectedEntity = "";
        private static List<List<string>> queryRes = new List<List<string>> { new List<string> { "Запрос не был сделан" } };
        public DBQueryController(AppDBContext appDB, IАвтор iA, IТехника iTq, IСостояние iCnd,
            IСтатусКартины iStp, IСтрана iCt, IЖанр iJ, IСтиль iSt, IСущности iSu, IКартина iPc,
            IРеставрация iR, IСвязь_Материал_Картина iMP, IСвязь_Рест_Вид iRT, IМатериал iM, IВид_реставрации iRtp,
            IМесто iPl, IЗал iRm, IЭкспозиция iEx, IЭкспонат iSh)
        {
            _Authors = iA;
            _Techs = iTq;
            _StatsP = iStp;
            _Conds = iCnd;
            _Countries = iCt;
            _Janres = iJ;
            _Styles = iSt;
            _Entities = iSu;
            _Pictures = iPc;
            _Materials = iM;
            _Restorations = iR;
            _Rest_Types = iRT;
            _Pic_Materials = iMP;
            _RestorationTypes = iRtp;
            _Showp = iSh;
            _Expos = iEx;
            _Rooms = iRm;
            _Places = iPl;
            db = appDB;
        }

        [HttpPost]
        public ViewResult QueryResult(string entity, string attribute, string operation, string val, string group, string agg)
        {
            ViewBag.Title = "Query result";
            queryRes = QueryBuild(entity, attribute, operation, val, group, agg);
            DBQueryViewModel obj = new DBQueryViewModel
            {
                allAuthors = _Authors.Authors,
                allTechniques = _Techs.Techniques,
                allCondit = _Conds.Conditions,
                allStatus = _StatsP.Statuses,
                allCountries = _Countries.Countries,
                allJanres = _Janres.Jenres,
                allStyles = _Styles.Styles,
                allEntities = _Entities.Entities,
                allEntityTypes = _Entities.EntityTypes,
                allPictures = _Pictures.Pictures,
                allMaterials = _Materials.Materials,
                allPic_Materials = _Pic_Materials.Pic_Material,
                allRestorations = _Restorations.Restorations,
                allRest_Types = _Rest_Types.Rest_Types,
                allRestorationTypes = _RestorationTypes.Restoration_types,
                allExpos = _Expos.Expos,
                allPlaces = _Places.Places,
                allRooms = _Rooms.Rooms,
                allShowpieces = _Showp.Showpieces,
                selEnt = selectedEntity,
                queryResult = queryRes
            };
            return View(obj);
        }
        public ViewResult SaveQuery(string fileName)
        {
            ViewBag.Title = "Query Saved";
            ViewBag.filename = fileName;
            return View();
        }
        [HttpPost]
        public dynamic QueryAction(string action, string csv)
        {
            ViewBag.Title = "Query result";
            switch (action)
            {
                case "save":
                    {
                        csv = csv.Replace('↵', '\n');
                        string filename = $"SQLQueryResult{DateTime.Now.ToString().Replace(':', '-').Replace(' ', ',')}.csv";
                        System.IO.File.WriteAllTextAsync($"QueryResults/{filename}", csv, System.Text.Encoding.Unicode);
                        return Redirect($"/DBQuery/SaveQuery?fileName={filename}");
                    }
                case "newQuery":
                    {
                        return Redirect("/DBQuery/DBQuery");
                    }
            }
            DBQueryViewModel obj = new DBQueryViewModel
            {
                allAuthors = _Authors.Authors,
                allTechniques = _Techs.Techniques,
                allCondit = _Conds.Conditions,
                allStatus = _StatsP.Statuses,
                allCountries = _Countries.Countries,
                allJanres = _Janres.Jenres,
                allStyles = _Styles.Styles,
                allEntities = _Entities.Entities,
                allEntityTypes = _Entities.EntityTypes,
                allPictures = _Pictures.Pictures,
                allMaterials = _Materials.Materials,
                allPic_Materials = _Pic_Materials.Pic_Material,
                allRestorations = _Restorations.Restorations,
                allRest_Types = _Rest_Types.Rest_Types,
                allRestorationTypes = _RestorationTypes.Restoration_types,
                allExpos = _Expos.Expos,
                allPlaces = _Places.Places,
                allRooms = _Rooms.Rooms,
                allShowpieces = _Showp.Showpieces,
                selEnt = selectedEntity,
                queryResult = queryRes
            };
            return View(obj);
        }

        public List<List<string>> QueryBuild(string entity, string attribute, string operation, string val, string group, string agg)
        {
            selectedEntity = entity;
            List<List<string>> res = new List<List<string>>();
            try
            {

                using (SqlConnection connection = new SqlConnection(db.Database.GetConnectionString()))
                {

                    string sql = $"SELECT * FROM {entity} WHERE {attribute} {operation} '{val}'";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<string> row = new List<string>();
                            for (int x = 0; x < reader.FieldCount; x++)
                            {
                                row.Add(reader.GetName(x));
                            }
                            res.Add(row);
                            while (reader.Read())
                            {
                                row = new List<string>();
                                for (int x = 0; x < reader.FieldCount; x++)
                                {
                                    row.Add(reader.GetValue(x).ToString());
                                }
                                res.Add(row);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                res.Add(new List<string> { "Ошибка выполнения запроса:" });
                res.Add(new List<string> { e.Message });
            }
            //sqlQuery += "GROUP BY"
            //var entt = db.GetType().GetProperty(entity).GetValue(db);
            //var proprt = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.IsClass && t.Name == "AppDBContext").GetProperties().FirstOrDefault(p => p.Name == entity).PropertyType;
            // var proprt = typeof(DbSet<>).MakeGenericType(db.Картина.GetType()/*entt.GetType().GenericTypeArguments[0]*/) ;
            //   var ds= db.GetType().GetProperty(entity).PropertyType;
            //   dynamic targetTable = Activator.CreateInstance(proprt);            
            //dynamic targetTable = db.GetType().GetProperty(entity).GetValue(db);
            //db.Реставрация.GetType().GetGenericArguments();
            // IQueryable<object> res = (IQueryable<object>)typeof(RelationalQueryableExtensions).GetMethod("FromSqlRaw").MakeGenericMethod(entt.GetType().GenericTypeArguments[0]).Invoke(targetTable, new object[] { targetTable, sqlQuery, new object[] { } });
            //IQueryable<object> res = targetTable.FromSqlRaw(sqlQuery);
            //switch (entity)
            //{
            //    case "Автор": { res = db.Автор.FromSqlRaw(sqlQuery); break; }
            //    case "Вид_реставрации": { res = db.Вид_реставрации.FromSqlRaw(sqlQuery); break; }
            //    case "Жанр": { res = db.Жанр.FromSqlRaw(sqlQuery); break; }
            //    case "Зал": { res = db.Зал.FromSqlRaw(sqlQuery); break; }
            //    case "Картина": { res = db.Картина.FromSqlRaw(sqlQuery); break; }
            //    case "Материал": { res = db.Материал.FromSqlRaw(sqlQuery); break; }
            //    case "Место": { res = db.Место.FromSqlRaw(sqlQuery); break; }
            //    case "Реставрация": { res = db.Реставрация.FromSqlRaw(sqlQuery); break; }
            //    case "Связь_Материал_Картина": { res = db.Связь_Материал_Картина.FromSqlRaw(sqlQuery); break; }
            //    case "Связь_Рест_Вид": { res = db.Связь_Рест_Вид.FromSqlRaw(sqlQuery); break; }
            //    case "Состояние_картины": { res = db.Состояние_картины.FromSqlRaw(sqlQuery); break; }
            //    case "Статус_картины": { res = db.Статус_картины.FromSqlRaw(sqlQuery); break; }
            //    case "Статус_экспозиции": { res = db.Статус_экспозиции.FromSqlRaw(sqlQuery); break; }
            //    case "Стиль": { res = db.Стиль.FromSqlRaw(sqlQuery); break; }
            //    case "Страна": { res = db.Страна.FromSqlRaw(sqlQuery); break; }
            //    case "Техника": { res = db.Техника.FromSqlRaw(sqlQuery); break; }
            //    case "Экспозиция": { res = db.Экспозиция.FromSqlRaw(sqlQuery); break; }
            //    case "Экспонат": { res = db.Экспонат.FromSqlRaw(sqlQuery); break; }
            //}
            //DbSet<object> a = (DbSet<object>)db.GetType().GetProperty(entity).GetValue(db);
            //res = from ent in a
            //      group ent by ent.GetType().GetProperty(@group).GetValue(ent) 
            //      into gr
            //      select new { gr.Key, Count = gr.Count()};
            return res;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ViewResult DBQuery(string entity)
        {
            selectedEntity = entity;
            return DBQuery();
        }
        public ViewResult DBQuery()
        {
            ViewBag.Title = "Query management";
            DBQueryViewModel obj = new DBQueryViewModel
            {
                allAuthors = _Authors.Authors,
                allTechniques = _Techs.Techniques,
                allCondit = _Conds.Conditions,
                allStatus = _StatsP.Statuses,
                allCountries = _Countries.Countries,
                allJanres = _Janres.Jenres,
                allStyles = _Styles.Styles,
                allEntities = _Entities.Entities,
                allEntityTypes = _Entities.EntityTypes,
                allPictures = _Pictures.Pictures,
                allMaterials = _Materials.Materials,
                allPic_Materials = _Pic_Materials.Pic_Material,
                allRestorations = _Restorations.Restorations,
                allRest_Types = _Rest_Types.Rest_Types,
                allRestorationTypes = _RestorationTypes.Restoration_types,
                allExpos = _Expos.Expos,
                allPlaces = _Places.Places,
                allRooms = _Rooms.Rooms,
                allShowpieces = _Showp.Showpieces,
                selEnt = selectedEntity
            };
            return View(obj);
        }
    }
}
