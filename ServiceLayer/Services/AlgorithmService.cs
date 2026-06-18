using ServiceLayer.Models;

namespace ServiceLayer.Services
{
    public class AlgorithmService
    {
        public List<AlgorithmState> GenerateBubbleSortSteps(int[] array)
        {
            var steps = new List<AlgorithmState>();
            int[] arr = (int[])array.Clone();
            int n = arr.Length;
            steps.Add(CreateState(arr, Array.Empty<int>(), 0, "Начальное состояние"));

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    steps.Add(CreateState(arr, new[] { j, j + 1 }, 1, $"Сравниваем {arr[j]} и {arr[j + 1]}"));
                    if (arr[j] > arr[j + 1])
                    {
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                        steps.Add(CreateState(arr, new[] { j, j + 1 }, 2, $"Меняем местами {arr[j]} и {arr[j + 1]}"));
                    }
                }
            }

            var finalState = CreateState(arr, Array.Empty<int>(), 3, "Сортировка завершена");
            finalState.IsCompleted = true;
            steps.Add(finalState);
            return steps;
        }

        public List<AlgorithmState> GenerateInsertionSortSteps(int[] array)
        {
            var steps = new List<AlgorithmState>();
            int[] arr = (int[])array.Clone();
            steps.Add(CreateState(arr, System.Array.Empty<int>(), 0, "Начальное состояние"));

            for (int i = 1; i < arr.Length; i++)
            {
                int key = arr[i];
                int j = i - 1;
                steps.Add(CreateState(arr, new[] { i }, 1, $"Выбрали элемент {key} на позиции {i}"));

                while (j >= 0 && arr[j] > key)
                {
                    int movedValue = arr[j];
                    // Сначала показываем два элемента: исходный и тот, куда переносим
                    steps.Add(CreateState(arr, new[] { j, j + 1 }, 2,
                        $"Переносим {movedValue} с позиции {j} на позицию {j + 1}"));

                    arr[j + 1] = arr[j];
                    arr[j] = -1; // Временно помечаем как "пусто"

                    steps.Add(CreateState(arr, new[] { j, j + 1 }, 2,
                        $"Освободили позицию {j} для вставки {key}"));
                    j--;
                }

                arr[j + 1] = key;
                steps.Add(CreateState(arr, new[] { j + 1 }, 3, $"Вставили {key} на позицию {j + 1}"));
            }

            // Убираем все -1 (их быть не должно в конце, но на всякий случай)
            var final = CreateState(arr.Where(x => x != -1).ToArray(), System.Array.Empty<int>(), 4, "Сортировка завершена");
            final.IsCompleted = true;
            steps.Add(final);
            return steps;
        }

        public List<AlgorithmState> GenerateLinearSearchSteps(int[] array, int target)
        {
            var steps = new List<AlgorithmState>();
            steps.Add(CreateState(array, Array.Empty<int>(), 0, $"Ищем {target}"));
            for (int i = 0; i < array.Length; i++)
            {
                steps.Add(CreateState(array, new[] { i }, 1, $"Проверяем элемент на позиции {i}"));
                if (array[i] == target)
                {
                    var foundState = CreateState(array, new[] { i }, 2, $"Нашли {target} на позиции {i}");
                    foundState.IsCompleted = true;
                    steps.Add(foundState);
                    return steps;
                }
            }
            var notFoundState = CreateState(array, Array.Empty<int>(), 3, "Элемент не найден");
            notFoundState.IsCompleted = true;
            steps.Add(notFoundState);
            return steps;
        }

        public List<AlgorithmState> GenerateBinarySearchSteps(int[] sortedArray, int target)
        {
            var steps = new List<AlgorithmState>();
            int[] arr = (int[])sortedArray.Clone();
            steps.Add(CreateState(arr, Array.Empty<int>(), 0, $"Ищем {target}"));
            int left = 0, right = arr.Length - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                steps.Add(CreateState(arr, new[] { left, right, mid }, 1, $"Сравниваем средний элемент {arr[mid]} на позиции {mid}"));
                if (arr[mid] == target)
                {
                    var foundState = CreateState(arr, new[] { mid }, 2, $"Нашли {target} на позиции {mid}");
                    foundState.IsCompleted = true;
                    steps.Add(foundState);
                    return steps;
                }
                else if (arr[mid] > target)
                {
                    right = mid - 1;
                    steps.Add(CreateState(arr, new[] { left, right }, 3, "Ищем в левой половине"));
                }
                else
                {
                    left = mid + 1;
                    steps.Add(CreateState(arr, new[] { left, right }, 4, "Ищем в правой половине"));
                }
            }
            var notFoundState = CreateState(arr, Array.Empty<int>(), 5, "Элемент не найден");
            notFoundState.IsCompleted = true;
            steps.Add(notFoundState);
            return steps;
        }

        private AlgorithmState CreateState(int[] arr, int[] highlights, int line, string comment)
        {
            return new AlgorithmState
            {
                Array = (int[])arr.Clone(),
                HighlightedIndices = highlights,
                CurrentLine = line,
                Comment = comment
            };
        }
    }
}
