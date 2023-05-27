using System.Linq.Expressions;

namespace ExpressionSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Expression Tree(式木)
            // 式(数式)を木構造で表現したもの
            // コンパイルおよび実行可能であり、実行可能コードの動的な作成・変更が可能

            // ラムダ式をExpression型の変数に割り当てると式木としてコンパイルされる
            // 以下のコードは同じ意味となる
            // ①
            Expression<Func<int, int>> exp1 = x => x + 5;
            // ②
            // 引数xの式を作成
            var argX = Expression.Parameter(typeof(int), "x");
            // x + 5の式を作成
            var body = Expression.Add(argX, Expression.Constant(5));
            // x => x + 5の式を作成
            var exp2 = Expression.Lambda<Func<int, int>>(body, argX);

            // InstanceCreator<T>の利用
            var personInstance = InstanceCreator<Person>.Create;
            var person = new Person();
            PrintPersonName(personInstance);
            Console.WriteLine(person.Name);

            // 条件分岐で動的に式を変更する例
            var x = Expression.Parameter(typeof(int), "x");
            BinaryExpression binaryBody;
            if (args.Length > 0)
            {
                binaryBody = Expression.Multiply(x, Expression.Constant(int.Parse(args[0])));
            }
            else
            {
                binaryBody = Expression.Add(x, Expression.Constant(1));
            }
            var exp3 = Expression.Lambda<Func<int, int>>(binaryBody, x).Compile();
            PrintUnaryOperatorResult(exp3, 3);

            // Orderby条件
            var personModels = new List<PersonModel>
            {
                new PersonModel { Name = "CCC" },
                new PersonModel { Name = "AAA" },
                new PersonModel { Name = "BBB" }
            };
            var lambda = CreateOrderByLambda("Name");

            IQueryable<PersonModel> query = personModels.AsQueryable();
            var sortedList = query.OrderByDescending(lambda).ToList();
            foreach (var model in sortedList)
            {
                Console.WriteLine(model.Name);
            }

        }

        public static void PrintUnaryOperatorResult(Func<int, int> func, int arg)
        {
            Console.WriteLine(func(arg));
        }

        public static void PrintPersonName(Func<Person> func)
        {
            Console.WriteLine(func().Name);
        }

        public static Expression<Func<PersonModel, object>> CreateOrderByLambda(string property)
        {
            var arg_personModelType = Expression.Parameter(typeof(PersonModel), "model");
            var body_property = Expression.Property(arg_personModelType, property);
            var lambda = Expression.Lambda<Func<PersonModel, object>>(Expression.Convert(body_property, typeof(object)), arg_personModelType);
            return lambda;
        }
    }

    /// <summary>
    /// 式木を使用したインスタンス生成<br></br>
    /// 式木を作成した後コンパイルしてデリゲートとして返却
    /// </summary>
    /// <typeparam name="T">インスタンスの型</typeparam>
    public static class InstanceCreator<T>
    {
        public static Func<T> Create => CreateInstance();
        private static Func<T> CreateInstance()
        {
            return Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }
    }

    public class Person
    {
        public string Name => "aaa";
    }

    public class PersonModel
    {
        public string Name { get; set; } = null!;
    }
}