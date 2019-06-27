using ArkBridgeSharedEntities.Entities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkWebMapMasterServer.Services
{
    public static class ArrayActionsHandler
    {
        public delegate Task ArrayActionsCreate<T, O>(Microsoft.AspNetCore.Http.HttpContext e, O context);
        public delegate Task ArrayActionsSelectGet<T, O>(Microsoft.AspNetCore.Http.HttpContext e, T item, O context);
        public delegate Task ArrayActionsSelectPost<T, O>(Microsoft.AspNetCore.Http.HttpContext e, T item, O context);
        public delegate Task ArrayActionsSelectDelete<T, O>(Microsoft.AspNetCore.Http.HttpContext e, T item, O context);

        public static Task OnHttpRequest<T, O>(Microsoft.AspNetCore.Http.HttpContext e, string path, O context, LiteCollection<T> collec, ArrayActionsCreate<T, O> create, ArrayActionsSelectGet<T, O> selectGet, ArrayActionsSelectPost<T, O> selectPost, ArrayActionsSelectDelete<T, O> selectDelete)
        {
            var method = Program.FindRequestMethod(e);

            //If the action is "@new", create a new one
            if (path == "@new" && method == RequestHttpMethod.post)
                return create(e, context);

            //Find it in the collection by ID
            T entry = collec.FindById(path);
            if(entry != null)
            {
                if (method == RequestHttpMethod.get)
                    return selectGet(e, entry, context);
                if (method == RequestHttpMethod.post)
                    return selectPost(e, entry, context);
                if (method == RequestHttpMethod.delete)
                    return selectDelete(e, entry, context);
            }

            //Throw not found
            throw new StandardError("Array Element Not Found", StandardErrorCode.NotFound);
        }
    }
}
