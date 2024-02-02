using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
   public class SharedObjectContext<T>
   {
      private readonly T context;

      #region Singleton Pattern

      // Static members are lazily initialized.
      // .NET guarantees thread safety for static initialization.
      private static readonly SharedObjectContext<T> instance = new SharedObjectContext<T>();

      // Make the constructor private to hide it. 
      // This class adheres to the singleton pattern.
      private SharedObjectContext()
      {
         // Create the ObjectContext.
         context = (T)Activator.CreateInstance(typeof(T));
      }

      // Return the single instance of the ClientSessionManager type.
      public static SharedObjectContext<T> Instance
      {
         get
         {
            return instance;
         }
      }

      #endregion

      public T Context
      {
         get
         {
            return context;
         }
      }
   }
}
