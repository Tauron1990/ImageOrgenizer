

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WindowsFormsApplication1;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.ImageOrganizer.Data;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace TestApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string path = Path.Combine(@"I:\Sankaku Safe Test", "Main - Test.imgdb");

            using (var db = new DatabaseImpl(path))
            {
                StringBuilder commandBuilder = new StringBuilder();
                List<string> ids = new List<string>();
                
                var temp = db.Images.OrderBy(i => i.Name).ToArray();

                foreach (var suElements in Split(temp, 7000).Select(sel => sel.ToArray()))
                {
                    ids.Clear();
                    commandBuilder.Clear();

                    commandBuilder//.AppendLine("BEGIN TRANSACTION;")
                        .AppendLine("Update Images")
                        .AppendLine("SET SortOrder = Case Id");

                    for (var index = 0; index < suElements.Length; index++)
                    {
                        var entity = suElements[index];
                        ids.Add(entity.Id.ToString());
                        commandBuilder.AppendLine($"    WHEN {entity.Id} THEN '{index}'");
                    }

                    commandBuilder.AppendLine("END").Append("WHERE Id IN(");

                    commandBuilder.Append(string.Join(", ", ids));
                    
                    commandBuilder.AppendLine(")");

                    //commandBuilder.AppendLine("COMMIT;");

                    using (var trans = db.Database.BeginTransaction())
                    {
                        var command = commandBuilder.ToString();
                        var count = db.Database.ExecuteSqlCommand(new RawSqlString(command));
                        trans.Commit();
                    }
                }


            }

            //UPDATE cityd
            //SET time_zone = CASE locId
            //    WHEN 173567 THEN '-7.000000'
                //WHEN 173568 THEN '-8.000000'
                //WHEN 173569 THEN '-6.000000'
                //WHEN 173570 THEN '-5.000000'
                //WHEN 173571 THEN '-6.000000'
            //END
            //    WHERE   locId IN(173567, 173568, 173569, 173570, 173571)

            Console.Write("Fertig");
            Console.ReadKey();
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}
