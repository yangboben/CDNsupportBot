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


       private bool contain(string want, string have) {
  
           foreach (string i in have.Split('/')) {
               if (want == i)
                   return true;
           }
         return false;
       }
                   
            //return true means find the right
       public bool select<T>(string target,string target_value, IEnumerable<T> options , out IEnumerable<T> changedoptions, out List<string> option_provide)
       {

           IEnumerable<T> temp = options.Where(p => (contain((string)(p.GetType().GetProperty(target).GetValue(p)),target_value))).ToList();
           
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


       public abstract  string getAskString(PropertyInfo currentitem);
       
    }

    [Serializable]
    class HowQuestionInfo : QuestionInfo 
    {

       [Order(0)]
       public string service { get; set; }

       [Order(1)]
       public string item { get; set; }
       
       public HowQuestionInfo() {
            intent = "how";
        }

       public override string getAskString(PropertyInfo currentitem)
       {

           string r = "";
           switch (currentitem.Name) { 
               case "service":
                   r = string.Format("您能更清楚的描述下你是想了解对什么的配置问题么");
                   break;
               case "item":
                   r = string.Format("您是想了解关于如何配置{0}的哪一项?");
                   break;
               default :
                   r = "我好像脑子出错了。您不如刷新下或者咨询人工客服吧";
                   break;
           }
           return r;
       }
    }

    [Serializable]
    class HowMuchQuestionInfo : QuestionInfo {

        [Order(0)]
        public string service { get; set; }

        public HowMuchQuestionInfo() {
            intent = "how_much";
        }

        public override string getAskString(PropertyInfo currentitem)
        {
            string r = String.Format("您能更清楚的描述下您是想了解什么的价格么？");
            return r;
        }
     
    
    }

    [Serializable]
    class HowLongQuestionInfo : QuestionInfo {

        [Order(0)]
        public string service { get; set; }

        [Order(1)]
        public string action { get; set; }

        public HowLongQuestionInfo() {
            intent = "how_long";
        }

        public override string getAskString(PropertyInfo currentitem)
        {
            string r = "";
            switch (currentitem.Name) {
                case "service": String.Format("您能更清楚的描述下你是想了解什么服务么？");
                    break;
                case "action": String.Format("您是想了解关于{0}进行什么操作需要的时间?",service);
                    break;
                default:
                    r = "我好像脑子出错了。您不如刷新下或者咨询人工客服吧";
                    break;
            }

            return r;
        }
   
    }

   

    [Serializable]
    class WhatQuestionInfo : QuestionInfo
    {
        [Order(0)]
        public string range { get; set; }

        public WhatQuestionInfo()
        {
            intent = "what";
        }

        public override string getAskString(PropertyInfo currentitem)
        {
            string r="";
            if (range != null)  
             r = string.Format("您想咨询的是有关 {0} 的问题么?",range);
            r += "我们现在可能并没有这方面的信息.";
            return r; 
        }
    }

    [Serializable]
    class HaveQuestionInfo : QuestionInfo
    {
        [Order(0)]
        public string service { get; set; }

        public HaveQuestionInfo() {
            intent = "have";
        }

        public override string getAskString(PropertyInfo currentitem)
        {
            string r = "";
            if (service != null)
                r = string.Format(" 我们没有 {0} 这类服务。", service);
            else
                r = string.Format("这个我也不清楚，要不您跟我们的人工客服联系一下吧"); 
            return r;
        }
    }

    [Serializable]
    class HowManyQuestionInfo : QuestionInfo
    {
        [Order(0)]
        public string service {get;set;}

        [Order(1)]
        public string item { get; set; }
        [Order(2)]
        public string company { get; set; }

        public HowManyQuestionInfo()
        {
            intent = "how_many";
        }

        public override string getAskString(PropertyInfo currentitem)
        {
            string r = "";
            
            switch(currentitem.Name){
                case "service": r = "您能更清楚的描述下你是想了解什么服务么？";
                    break;
                case "item": r = "您是想了解关于{0}的哪一项?";
                    break;
                case "company": r = "您是想了解关于哪一家公司的呢?";
                    break;
            }

            return r;
        }
    }
}