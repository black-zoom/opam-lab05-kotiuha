using System;
using System.Collections.Generic;
using System.Globalization;

class IntStack
{
    private int[] items;
    public int Count { get; private set; }
    public int Capacity => items.Length;

    public IntStack(int capacity = 3)
    {
        items = new int[capacity];
    }

    public void Push(int x)
    {
        if (Count == Capacity)
        {
            int[] newItems = new int[Capacity * 2];
            Array.Copy(items, newItems, Count);
            items = newItems;
        }

        items[Count++] = x;
    }

    public int Pop()
    {
        if (Count == 0)
            throw new InvalidOperationException("Stack is empty");

        return items[--Count];
    }

    public int Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("Stack is empty");

        return items[Count - 1];
    }
}

class ServiceRequest
{
    public int Id;
    public int Weight;
    public int Remaining;
    public int StartTime = -1;
    public int FinishTime = -1;
}

class Program
{
    const int N = 14;
    const int K = 1;
    const int A = N % 3;
    const int B = N % 4;
    const int C = N % 5;

    static bool IsOpening(char ch)
    {
        return ch == '(' || ch == '[' || ch == '{';
    }

    static bool IsMatchingPair(char open, char close)
    {
        return (open == '(' && close == ')') ||
               (open == '[' && close == ']') ||
               (open == '{' && close == '}');
    }

    static bool CheckBrackets(string text, out int errorIndex)
    {
        Stack<(char Symbol, int Index)> stack = new Stack<(char, int)>();
        bool inQuotes = false;
        errorIndex = -1;

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            if (inQuotes)
            {
                if (ch == '\\' && i + 1 < text.Length)
                {
                    i++;
                    continue;
                }

                if (ch == '"')
                    inQuotes = false;

                continue;
            }

            if (ch == '"')
            {
                inQuotes = true;
                continue;
            }

            if (IsOpening(ch))
            {
                stack.Push((ch, i));
            }
            else if (ch == ')' || ch == ']' || ch == '}')
            {
                if (stack.Count == 0)
                {
                    errorIndex = i;
                    return false;
                }

                var opening = stack.Pop();

                if (!IsMatchingPair(opening.Symbol, ch))
                {
                    errorIndex = i;
                    return false;
                }
            }
        }

        if (stack.Count > 0)
        {
            while (stack.Count > 0)
            {
                errorIndex = stack.Pop().Index;
            }

            return false;
        }

        return true;
    }

    static bool CheckBracketsCustom(string text, out int errorIndex)
    {
        IntStack symbols = new IntStack();
        IntStack positions = new IntStack();
        bool inQuotes = false;
        errorIndex = -1;

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            if (inQuotes)
            {
                if (ch == '\\' && i + 1 < text.Length)
                {
                    i++;
                    continue;
                }

                if (ch == '"')
                    inQuotes = false;

                continue;
            }

            if (ch == '"')
            {
                inQuotes = true;
                continue;
            }

            if (IsOpening(ch))
            {
                symbols.Push(ch);
                positions.Push(i);
            }
            else if (ch == ')' || ch == ']' || ch == '}')
            {
                if (symbols.Count == 0 || positions.Count == 0)
                {
                    errorIndex = i;
                    return false;
                }

                char opening = (char)symbols.Pop();
                positions.Pop();

                if (!IsMatchingPair(opening, ch))
                {
                    errorIndex = i;
                    return false;
                }
            }
        }

        if (symbols.Count > 0 || positions.Count > 0)
        {
            while (symbols.Count > 0 && positions.Count > 0)
            {
                errorIndex = positions.Pop();
                symbols.Pop();
            }

            return false;
        }

        return true;
    }

    static void Task1()
    {
        Console.WriteLine("Task 1");

        string[] tests =
        {
            "(a[b]{c})",
            "([)]",
            ")(",
            "((",
            "",
            "(\"[\")",
            "(",
            "abc"
        };

        foreach (string test in tests)
        {
            bool result = CheckBrackets(test, out int index);
            Console.WriteLine(
                $"\"{test}\" -> {(result ? "balanced" : "unbalanced")}, index={index}");
        }

        Console.WriteLine();
    }

    static bool TryEvaluateRpn(string expression, out double result, out string error)
    {
        Stack<double> stack = new Stack<double>();
        result = 0;
        error = "";

        string[] tokens = expression.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            error = "Empty expression";
            return false;
        }

        foreach (string token in tokens)
        {
            if (double.TryParse(
                token,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double number))
            {
                stack.Push(number);
                continue;
            }

            if (token != "+" && token != "-" && token != "*" &&
                token != "/" && token != "^")
            {
                error = $"Unknown token: {token}";
                return false;
            }

            if (stack.Count < 2)
            {
                error = "Not enough operands";
                return false;
            }

            double right = stack.Pop();
            double left = stack.Pop();
            double value;

            switch (token)
            {
                case "+":
                    value = left + right;
                    break;
                case "-":
                    value = left - right;
                    break;
                case "*":
                    value = left * right;
                    break;
                case "/":
                    if (right == 0)
                    {
                        error = "Division by zero";
                        return false;
                    }

                    value = left / right;
                    break;
                case "^":
                    value = Math.Pow(left, right);
                    break;
                default:
                    error = "Invalid operation";
                    return false;
            }

            stack.Push(value);
        }

        if (stack.Count != 1)
        {
            error = "Too many operands";
            return false;
        }

        result = stack.Pop();
        return true;
    }

    static void Task2()
    {
        Console.WriteLine("Task 2");

        string[] tests =
        {
            "3 4 + 2 *",
            "5 3 -",
            "8 2 /",
            "2 3 4 * +",
            "5 0 /",
            "2 3 + 4 * 5 - 15 /",
            "5 2 %",
            "+",
            "5",
            "2 3",
            ""
        };

        foreach (string expression in tests)
        {
            if (TryEvaluateRpn(expression, out double result, out string error))
            {
                Console.WriteLine(
                    $"{expression} = {result.ToString(CultureInfo.InvariantCulture)}");
            }
            else
            {
                Console.WriteLine($"{expression} -> Error: {error}");
            }
        }

        Console.WriteLine();
    }

    static void SimulateService(int requestCount)
    {
        Queue<ServiceRequest> queue = new Queue<ServiceRequest>();
        ServiceRequest[] requests = new ServiceRequest[requestCount];
        Random random = new Random(N);

        for (int i = 0; i < requestCount; i++)
        {
            int weight = random.Next(1, 10);

            ServiceRequest request = new ServiceRequest
            {
                Id = i + 1,
                Weight = weight,
                Remaining = weight
            };

            requests[i] = request;
            queue.Enqueue(request);
        }

        int maxQueueLength = queue.Count;
        int currentTime = 0;
        int quantum = K % 3 + 2;

        while (queue.Count > 0)
        {
            if (queue.Count == 0)
                break;

            ServiceRequest request = queue.Dequeue();

            if (request.StartTime == -1)
                request.StartTime = currentTime;

            int processed = Math.Min(quantum, request.Remaining);

            currentTime += processed;
            request.Remaining -= processed;

            if (request.Remaining > 0)
            {
                queue.Enqueue(request);

                if (queue.Count > maxQueueLength)
                    maxQueueLength = queue.Count;
            }
            else
            {
                request.FinishTime = currentTime;
            }
        }

        foreach (ServiceRequest request in requests)
        {
            Console.WriteLine(
                $"Request {request.Id}: weight={request.Weight}, " +
                $"start={request.StartTime}, finish={request.FinishTime}");
        }

        Console.WriteLine($"Max queue length: {maxQueueLength}");
    }


    static void Task3()
    {
        Console.WriteLine("Task 3");

        Console.WriteLine("Zero requests test:");
        SimulateService(0);

        Console.WriteLine("Regular simulation:");
        SimulateService(8 + K % 5);

        Console.WriteLine();
    }

    static void Task4()
    {
        Console.WriteLine("Task 4");

        IntStack stack = new IntStack(K % 4 + 2);
        int pushes = K % 4 + 3;

        for (int i = 1; i <= pushes; i++)
        {
            stack.Push(i);
            Console.WriteLine(
                $"Push {i}: Count={stack.Count}, Capacity={stack.Capacity}");
        }

        Console.WriteLine($"Peek: {stack.Peek()}");

        bool matches = true;

        string[] tests =
        {
            "(a[b]{c})",
            "([)]",
            ")(",
            "((",
            "",
            "(\"[\")",
            "("
        };

        foreach (string test in tests)
        {
            bool standardResult = CheckBrackets(test, out int standardIndex);
            bool customResult = CheckBracketsCustom(test, out int customIndex);

            if (standardResult != customResult || standardIndex != customIndex)
                matches = false;
        }

        Console.WriteLine(
            $"Custom stack bracket tests: {(matches ? "OK" : "FAILED")}");

        IntStack emptyStack = new IntStack();

        try
        {
            emptyStack.Pop();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Pop on empty stack: error handled");
        }
    }

    static void Main()
    {
        Task1();
        Task2();
        Task3();
        Task4();
    }
}
