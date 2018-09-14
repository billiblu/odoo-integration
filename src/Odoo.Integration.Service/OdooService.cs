using CookComputing.XmlRpc;
using Odoo.Integration.Service.Attributes;
using Odoo.Integration.Service.Interfaces;
using Odoo.Integration.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Odoo.Integration.Service
{
    public class OdooService
    {
        private const string LoginPath = "/xmlrpc/2/common";
        private const string DataPath = "/xmlrpc/2/object";

        private readonly OdooConnection _connection;
        private readonly OdooContext _context;

        public OdooService(OdooConnection connection)
        {
            _connection = connection;
            _context = new OdooContext();

            _context.OdooAuthentication = XmlRpcProxyGen.Create<IOdooCommon>();
            _context.OdooAuthentication.Url = string.Format(@"{0}{1}", _connection.Url, LoginPath);

            _context.OdooData = XmlRpcProxyGen.Create<IOdooObject>();
            _context.OdooData.Url = string.Format(@"{0}{1}", _connection.Url, DataPath);

            _context.Database = connection.Database;
            _context.Username = connection.Username;
            _context.Password = connection.Password;
            Login();
        }

        public void Login()
        {
            try
            {
                _context.UserId = _context.OdooAuthentication.Login(_connection.Database, _connection.Username, _connection.Password);
            }
            catch (XmlRpcFaultException ex)
            {
                throw new Exception("Login failed, XmlRpc Error", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Login failed, Error", ex);
            }
        }

        public int Create(string model, XmlRpcStruct fieldValues)
        {
            return _context.OdooData.Create(_connection.Database, _context.UserId, _connection.Password, model, "create", fieldValues);
        }

        public XmlRpcStruct ButtonProformaVoucher(string model, int[] ids)
        {
            return _context.OdooData.ButtonProformaVoucher(_connection.Database, _context.UserId, _connection.Password, model, "button_proforma_voucher", ids);
        }

        public int[] Search(string model, object[] filter, int? offset = null, int? limit = null)
        {
            return _context.OdooData.Search(_connection.Database, _context.UserId, _connection.Password, model, "search", filter, offset, limit);
        }

        public int Count(string model, object[] filter)
        {
            return _context.OdooData.Count(_connection.Database, _context.UserId, _connection.Password, model, "count", filter);
        }

        public XmlRpcStruct[] Read(string model, int[] ids, string[] fields)
        {
            return _context.OdooData.Read(_connection.Database, _context.UserId, _connection.Password, model, "read", ids, fields);
        }

        public XmlRpcStruct[] SearchAndRead(string model, object[] filter, string[] fields, int? offset = null, int? limit = null)
        {
            return _context.OdooData.SearchAndRead(_connection.Database, _context.UserId, _connection.Password, model, "search_read", filter, fields, offset, limit);
        }

        public List<T> SearchAndRead<T>(object[] filter, int? offset = null, int? limit = null)
        {
            List<T> toRet = new List<T>();
            Type classToBindType = typeof(T);
            Dictionary<string, string> fields = new Dictionary<string, string>();

            //the odoo model name is configured with attribute on T class
            OdooModelName odooModelName = classToBindType.GetCustomAttribute<OdooModelName>();
            if (odooModelName != null)
            {
                // I build a list of odoo field to make the request
                // I ignore the fields with odooIgnore attribute and check the OdooFieldName attribute to map cprrectly odoo fields to property of T class
                System.Reflection.PropertyInfo[] properties = classToBindType.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    OdooIgnore ignoreAttribute = property.GetCustomAttribute<OdooIgnore>();
                    if (ignoreAttribute == null)
                    {
                        OdooFieldName fieldNameAttribute = property.GetCustomAttribute<OdooFieldName>();

                        if (fieldNameAttribute == null)
                        {
                            fields.Add(property.Name, property.Name);
                        }
                        else
                        {
                            fields.Add(fieldNameAttribute.fieldName, property.Name);
                        }
                    }
                }

                XmlRpcStruct[] results = _context.OdooData.SearchAndRead(_connection.Database, _context.UserId, _connection.Password, odooModelName.modelName, "search_read", filter, fields.Keys.ToArray(), offset, limit);

                // scorro la lista dei risultati
                foreach (XmlRpcStruct resultItem in results)
                {
                    // I build an istance of T class
                    T itemToAdd = (T)Activator.CreateInstance(typeof(T));

                    // using fields dictionary I map every Odoo field to relative property of T class
                    var valuesReaded = resultItem.GetEnumerator();
                    while (valuesReaded.MoveNext())
                    {
                        //If Odoo request return additional fields (id for example) I ignore there fields if not present in T class
                        if (fields.Keys.Contains(valuesReaded.Key.ToString()))
                        {
                            PropertyInfo propToSet = classToBindType.GetProperty(fields[valuesReaded.Key.ToString()]);

                            if (propToSet != null)
                            {
                                //if (!(propToSet.PropertyType != typeof(System.Boolean) && valuesReaded.Value.GetType() == typeof(System.Boolean)))
                                if (propToSet.PropertyType == valuesReaded.GetType())
                                {
                                    if (propToSet.PropertyType == typeof(System.DateTime))
                                    {
                                        DateTime d = DateTime.Parse(valuesReaded.Value.ToString());
                                        if (d.Kind == DateTimeKind.Unspecified)
                                        {
                                            d = new DateTime(d.Ticks, DateTimeKind.Utc);
                                        }

                                        propToSet.SetValue(itemToAdd, d);
                                    }
                                    else if (propToSet.PropertyType == typeof(Tuple<int,string>))
                                    {
                                        Tuple<int, string> f = new Tuple<int, string>((int)((object[])valuesReaded.Value)[0], ((object[])valuesReaded.Value)[1].ToString());

                                        //f.Id = (int)((object[])valuesReaded.Value)[0];
                                        //f.Description = ((object[])valuesReaded.Value)[1].ToString();

                                        propToSet.SetValue(itemToAdd, f);
                                    }
                                    else
                                    {
                                        propToSet.SetValue(itemToAdd, valuesReaded.Value);
                                    }
                                }
                            }
                        }
                    }
                    toRet.Add(itemToAdd);
                }
            }

            return toRet;
        }


        public bool Remove(string model, int[] ids, string[] fields)
        {
            return _context.OdooData.Unlink(_connection.Database, _context.UserId, _connection.Password, model, "unlink", ids);
        }

        public bool Update(string model, int[] ids, XmlRpcStruct fields)
        {
            return _context.OdooData.Write(_connection.Database, _context.UserId, _connection.Password, model, "write", ids, fields);
        }
    }
}