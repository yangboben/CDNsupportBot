using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Bot.Builder.Luis.Models; // Namespace for Table storage types
using Microsoft.Bot.Builder.Dialogs;
using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
namespace CDNSupport
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
      Inherited = true, AllowMultiple = false)]
    [ImmutableObject(true)]
    public sealed class OrderAttribute : Attribute
    {
        private readonly int order;
        public int Order { get { return order; } }
        public OrderAttribute(int order) { this.order = order; }

   
    }

    [Serializable]
   public abstract class QuestionInfo
    {
        public QuestionInfo() { }
        public string intent { get; set; }
       [Order(0)]
        public string service { get; set; }


       abstract public void getAllinfo(LuisResult result);

       public IOrderedEnumerable<PropertyInfo> GetSortedProperties()
       {
           return this.GetType()
             .GetProperties()
             .Where(p => p.GetCustomAttribute(typeof(OrderAttribute), false) != null)
             .OrderBy(p => ((OrderAttribute)p.GetCustomAttribute(typeof(OrderAttribute), false)).Order);
       }

       public List<PropertyInfo> getSortedPropertiesList() {
           return GetSortedProperties().ToList();
       }

        //return true means find the right
       
       public bool select<T>(string target,string target_value, IEnumerable<T> options , out IEnumerable<T> changedoptions, out List<string> option_provide)
       {
         
           IEnumerable<T> temp = options.Where(p => (string)(p.GetType().GetProperty(target).GetValue(p)) == target_value).ToList();
           
           if (temp.Count()==0)
           {
               changedoptions = options;
               option_provide = options.Select(p => (string)p.GetType().GetProperty(target).GetValue(p)).Distinct().ToList();
               return false;
           }
           else
           {
               changedoptions = temp;
               option_provide = null;
               return true;
           }
           
       }
         
      
       
       
    }

    [Serializable]
    class CreateQusetionInfo : QuestionInfo 
    {
       [Order(1)]
       public string service_item { get; set; }

       [Order(2)]
       public string type { get; set; }
        
        public CreateQusetionInfo() {
            intent = "create";
        }
        

       // public string service { get; set;  }

        override public void getAllinfo(LuisResult result)
        {
            service = CDNsupportDialog.getEntity("service", result);
            
        }

        
    }

    [Serializable]

    class PriceQuestionInfo : QuestionInfo {

        public PriceQuestionInfo() {
            intent = "price";
        }
        public override void getAllinfo(LuisResult result)
        {

            service = CDNsupportDialog.getEntity("service", result);
            
        }
    
    }

    [Serializable]

    class DeployQuestionInfo : QuestionInfo {

        public DeployQuestionInfo() {
            intent = "deploy";
        }

        public override void getAllinfo(LuisResult result)
        {
            service = CDNsupportDialog.getEntity("service", result);
        }
    }

    [Serializable]
    class TroubleShootingQuestionInfo : QuestionInfo {

        [Order(1)]
        public string troubletype { get; set; }

        public TroubleShootingQuestionInfo() {
            intent = "troubleshooting";
        }

        public override void getAllinfo(LuisResult result)
        {
            service = CDNsupportDialog.getEntity("service", result);
            troubletype = CDNsupportDialog.getEntity("troubletype",result);
        }
    }
}