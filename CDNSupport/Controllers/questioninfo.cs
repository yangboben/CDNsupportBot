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


       public void transform(QuestionInfo question) {
           if (question == null)
               return;
           var list = question.getSortedPropertiesList();
           foreach (var i in GetSortedProperties()) {
               foreach (var j in list)
               {
                   if (i.Name == j.Name)
                       i.SetValue(this, j.GetValue(question));
               }
           }
       
       }


       public abstract  string getAskString();
       
    }

    [Serializable]
    class CreateQusetionInfo : QuestionInfo 
    {

       [Order(0)]
       public string service { get; set; }

       [Order(1)]
       public string service_item { get; set; }

       [Order(2)]
       public string type { get; set; }
        
       public CreateQusetionInfo() {
            intent = "create";
        }

       public override string getAskString()
       {

           string r = String.Format("您是要咨询关于创建{0} {1} {2}的问题么？",type,service,service_item);
          
           return r;
       }
    }

    [Serializable]

    class PriceQuestionInfo : QuestionInfo {

        [Order(0)]
        public string service { get; set; }

        public PriceQuestionInfo() {
            intent = "price";
        }

        public override string getAskString()
        {
            string r = String.Format("您咨询的是{0}的价格么？", service);
            return r;
        }
     
    
    }

    [Serializable]

    class DeployQuestionInfo : QuestionInfo {

        [Order(0)]
        public string service { get; set; }

        [Order(1)]
        public string item { get; set; }

        public DeployQuestionInfo() {
            intent = "config";
        }

        public override string getAskString()
        {
            string r = String.Format("您咨询的是有关配置 {0} {1} 的问题吗？", service, item);

            return r;
        }
   
    }

    [Serializable]
    class TroubleShootingQuestionInfo : QuestionInfo {

        [Order(0)]
        public string service { get; set; }

        [Order(1)]
        public string troubletype { get; set; }

        public TroubleShootingQuestionInfo() {
            intent = "troubleshooting";
        }

        public override string getAskString()
        {
            string r = String.Format("您咨询的是有关 {0} {1} 的问题么？", service, troubletype);
            return r;
        }
    }

    [Serializable]
    class AdvisoryQuestionInfo : QuestionInfo
    {
        [Order(1)]
        public string target { get; set; }

        public AdvisoryQuestionInfo()
        {
            intent = "advisory";
        }

        public override string getAskString()
        {
            string r="";
            if (target != null)  
             r = string.Format("您想咨询的是有关 {0} 的问题么?",target);
            r += "我们现在可能并没有这方面的信息.";
            return r; 
        }
    }
}