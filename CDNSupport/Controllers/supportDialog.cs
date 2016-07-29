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

    [LuisModel("9cc8dcc2-997d-42e2-88d7-3382a7b88a93", "61dd0a37d0194e86972e02f3bffb7a94")]
    [Serializable]
    public class CDNsupportDialog : LuisDialog<object>
    {

        enum Intent { Create = 1,Deploy,Change,Delete }

        public const string DefaultAnswer = "没有找到解决方法，建议联系我们的客服试试";

        public const string Entity_Service = "service";
        public const string Entity_Service_item = "service::item";
        public const string Emtity_type = "type";
        public const string Entity_target = "target";

        public const string Default_Type = "";
        public const string Default_Service_item = "";

        private QuestionInfo question;
        private IEnumerable<tableitem> allOptions;
       

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
           
            await context.PostAsync("请更详细的描述你的问题，或者联系人工客服");
            context.Wait(MessageReceived);
        }

        [LuisIntent("create")]
        public async Task QuestionCreate(IDialogContext context, LuisResult result)
        {

            question = new CreateQusetionInfo();
            
            getinfo(result);
           
           await getchoose(context).ConfigureAwait(false);
            
          
        }

        [LuisIntent("deploy")]
        public async Task QuestionDeploy(IDialogContext context, LuisResult result) {

            question = new DeployQuestionInfo();
            getinfo(result);
           await getchoose(context).ConfigureAwait(false);
            
        }

        [LuisIntent("change")]
        public async Task QuestionChange(IDialogContext context, LuisResult result) {

            await context.PostAsync("change");
            context.Wait(MessageReceived);
        }

        [LuisIntent("delete")]
        public async Task QuestionDelete(IDialogContext context, LuisResult result) {

            await context.PostAsync("delete");
            context.Wait(MessageReceived);
        }

        [LuisIntent("price")]
        public async Task QuestionPrice(IDialogContext context, LuisResult result) {

            question = new PriceQuestionInfo();
            getinfo(result);
            await getchoose(context).ConfigureAwait(false);
        }


        [LuisIntent("Troubleshooting")]
        public async Task QuestionTroubleShooting(IDialogContext context, LuisResult result) {

            question = new TroubleShootingQuestionInfo();
            getinfo(result);
           await getchoose(context).ConfigureAwait(false);
        }
        

        
        public static string getEntity(string entity_string, LuisResult result){

            EntityRecommendation entity;
            if (!result.TryFindEntity(entity_string, out entity))
            {
                return null;
            }

            return entity.Entity.Replace(" ","").ToLower() ;
        }


        

       private void getinfo(LuisResult result) {
           question.getAllinfo(result);
           allOptions = Storage.getAllOptions(question.intent);
           
       }
  

       public async Task getchoose(IDialogContext context)
       {
     
           List<string> option_provide = null;
           bool if_find = true;
           int offset = 0;
           foreach (var i in question.GetSortedProperties())
           {
               
               if (!question.select<tableitem>(i.Name, (string)i.GetValue(question), allOptions, out allOptions, out option_provide))
               {

                   if (allOptions.Count() == 1)
                   {                 
                       //TODO 一个问题结束
                       if_find = false;        
                   }

                   if (option_provide.Count() < 1)
                   {
                       //无可用的回答，转人工服务
                       await context.PostAsync("请联系客服").ConfigureAwait(false);
                       if_find = false;
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
                     
                       context.Call<string>(new ChooseDialog(offset, question, question.getSortedPropertiesList(), allOptions, option_provide), AfterChoose);
                       
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
               foreach (var i in allOptions)
               {
                   await context.PostAsync(i.answer).ConfigureAwait(false);
               }
               await context.PostAsync("您还有什么其他问题么").ConfigureAwait(false);
               context.Wait(MessageReceived);
           }
         
       }
      
       public async Task AfterChoose(IDialogContext context, IAwaitable<string> answer)
       {
     
           try
           {
               var temp = await answer;
              // await context.PostAsync(temp);
               if (temp != null) { 
                       
               }         
               else
               {
                   //await context.PostAsync("null");
                   //错误情况，用户返回null
               }
           }
           catch (Exception)
           {
           }

           
           
           //context.Reset();
           
           //getchoose(context);
                                 

       }

    

       public CDNsupportDialog(ILuisService service)
            : base(service)
        {
        }

       public CDNsupportDialog() { 
       }

     
    }
   

    
}
