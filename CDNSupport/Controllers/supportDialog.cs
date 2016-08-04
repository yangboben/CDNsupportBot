using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Reflection;
using System.Threading;
using CDNSupport.testclass;
using Microsoft.Bot.Connector;
namespace CDNSupport
{

    [LuisModel("fd644578-3651-469a-aa11-94f87f8c820f", "61dd0a37d0194e86972e02f3bffb7a94")]
    [Serializable]
    public class CDNsupportDialog : LuisDialog<object>
    {

        enum Intent { Create = 1,Deploy,Change,Delete }

        public const string DefaultAnswer = "没有找到解决方法，建议联系我们的客服试试";
      
        private QuestionInfo question;
        private IEnumerable<tableitem> allOptions;
        private PropertyInfo CurrentItem;
        
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            if (result.Query.ToLower() == "hi" || result.Query.ToLower() == "hello")
            {
                await context.PostAsync("您好,请问有什么可以帮您的么？");
                    context.Wait(MessageReceived);
                return;
            }
            if (getEntities(question, result))
            {
                //TODO:加入如果这个回答的entity仍然不是选项的那么返回的问题改变。
                await getchoose(context);
            }
            else
            {
                await context.PostAsync("请更详细的描述你的问题，或者联系人工客服");
                context.Wait(MessageReceived);
            
            }
        }

        [LuisIntent("how")]
        public async Task QuestionCreate(IDialogContext context, LuisResult result)
        {

            if (isReply(question, result) && question.GetType() == typeof(HowQuestionInfo))
            {
                //是上次询问问题的回答
                if (getEntities(question, result))
                {
                    await getchoose(context);
                }
                else
                {
                    await context.PostAsync(getNoAnswerReturn()).ConfigureAwait(false);
                    context.Wait(MessageReceived);
                }
            }
            else
            {
                QuestionInfo temp = question;
                question = new HowQuestionInfo();
                question.transform(temp);
                getinfo(question, result);
                await getchoose(context).ConfigureAwait(false);
            }
          
        }

        [LuisIntent("how_much")]
        public async Task QuestionDeploy(IDialogContext context, LuisResult result) {

            if (isReply(question, result) && question.GetType() == typeof(HowMuchQuestionInfo))
            {
                //是上次询问问题的回答
                if (getEntities(question, result))
                {
                    await getchoose(context);
                }
                else
                {
                    await context.PostAsync(getNoAnswerReturn()).ConfigureAwait(false);
                    context.Wait(MessageReceived);
                }
            }
            else
            {
                QuestionInfo temp = question;
                question = new HowMuchQuestionInfo();
                question.transform(temp);
                getinfo(question, result);
                await getchoose(context).ConfigureAwait(false);
            }
            
        }
        [LuisIntent("how_long")]
        public async Task QuestionPrice(IDialogContext context, LuisResult result) {
            if (isReply(question, result) && question.GetType() == typeof(HowLongQuestionInfo))
            {
                //是上次询问问题的回答
                if (getEntities(question, result))
                {
                    await getchoose(context);
                }
                else
                {
                    await context.PostAsync(getNoAnswerReturn()).ConfigureAwait(false);
                    context.Wait(MessageReceived);
                }
            }
            else
            {
                QuestionInfo temp = question;
                question = new HowLongQuestionInfo();
                question.transform(temp);
                getinfo(question, result);
                await getchoose(context).ConfigureAwait(false);
            }
        }


        [LuisIntent("what")]
        public async Task QuestionTroubleShooting(IDialogContext context, LuisResult result) {
            if (isReply(question, result) && question.GetType() == typeof(WhatQuestionInfo))
            {
                //是上次询问问题的回答
                if (getEntities(question, result))
                {
                    await getchoose(context);
                }
                else {
                    await context.PostAsync(getNoAnswerReturn()).ConfigureAwait(false);
                    context.Wait(MessageReceived);
                }
            }
            else
            {
                QuestionInfo temp = question;
                question = new WhatQuestionInfo();
                question.transform(temp);
                getinfo(question, result);
                await getchoose(context).ConfigureAwait(false);
            }
        }
          

        private string getEntity(string entity_string, LuisResult result)
        {

            EntityRecommendation entity;
            if (!result.TryFindEntity(entity_string, out entity))
            {
                return null;
            }

            return entity.Entity.Replace(" ", "").ToLower();
        }

        //true or false 表示是否是含有同类问题的追问或是否是回答,如果不含entity，返回fasle
        private bool getEntities(QuestionInfo question, LuisResult result)
        {
            if (question == null)
                return false;

            bool flag = false;
            foreach (var i in question.GetSortedProperties())
            {
                string entity = getEntity(i.Name, result);
                if (entity != null)
                {
                    i.SetValue(question, entity);
                    flag = true;
                }
            }
            return flag;
        }

        //判断是否是回答之前问题的(判断依据为是否有冲突的entity，如果有冲突则说明不是回答)
        private bool isReply(QuestionInfo question, LuisResult result){
            
            if (question == null)
                return false;

            foreach (var i in question.GetSortedProperties())
            {
                if (CurrentItem == i) {
                    break;
                }
                string entity = getEntity(i.Name, result);
                if (entity != null && (string)i.GetValue(question)!=entity)
                {
                    return false;
                }
            }
            return true;
        }

        //TODO: 确认是否包含提问的问题的回答
        private bool ifContainReply(QuestionInfo question, LuisResult result, PropertyInfo currentitem) {
             if(currentitem==null)
                  return false;
             try
             {
             }
             catch (Exception)
             {
                 return false;
             }
             return true;
        }



       private void getinfo(QuestionInfo question,LuisResult result) {
           getEntities(question, result);
           allOptions = Storage.getAllOptions(question.intent);
           
       }
  

       private async Task getchoose(IDialogContext context)
       {
     
           List<string> option_provide = null;
           bool if_find = true;
           int offset = 0;

           foreach (var i in question.GetSortedProperties())
           {
               CurrentItem = i;    
               if (!question.select<tableitem>(i.Name, (string)i.GetValue(question), allOptions, out allOptions, out option_provide))
               {

                   if (allOptions.Count() == 1)
                   {                 
                       //TODO: 一个问题结束
                       if_find = true;        
                   }

                   if (option_provide.Count() < 1)
                   {
                       //无可用的回答，转人工服务
                       await context.PostAsync(getNoAnswerReturn()).ConfigureAwait(false);
                       if_find = false;
                       break;
                   }
                   else if (option_provide.Count() == 1)
                   {
                       //只有一个选项供选择
                       i.SetValue(question, option_provide[0]);
                       question.select(i.Name, (string)i.GetValue(question), allOptions, out allOptions, out option_provide);
                       
                   }
                   else
                   {
                       //有多个选项 
                       //change
                       //context.Call<string>(new ChooseDialog(offset, question, question.getSortedPropertiesList(), allOptions, option_provide), AfterChoose);
                       await context.PostAsync(getAskString(option_provide));
                       if_find = false;
                       break;
                   }
               }
               if (allOptions.Count() == 1)
               {
                   if_find = true;
                   break;
               }
               offset++;
           }
           if (if_find)
           {
               CurrentItem = null;
               foreach (var i in allOptions)
               {
                   await context.PostAsync(i.answer).ConfigureAwait(false);
               }
               await context.PostAsync("您还有什么其他问题么").ConfigureAwait(false);
               context.Wait(MessageReceived);
           }
           else {

               context.Wait(MessageReceived);
           }
     
         
       }
      

       private string getNoAnswerReturn() {
           return "我可能没法帮你解决你的这个问题，为什么不去问问神奇海螺呢？";
       }


       private string getAskString(IEnumerable<string> options_provider) {
           string r = question.getAskString();
           r += "您的问题是不是和我们的";

           foreach (string i in options_provider) { 
               r += " "+i+",";
           }

           r += "其中之一有关呢？";

           return r;
       }


       public CDNsupportDialog(ILuisService service)
            : base(service)
        {
        }

       public CDNsupportDialog() { 
       }

     
    }
   

    
}
