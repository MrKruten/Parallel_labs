﻿# Лабораторная работа 2 - программа А

Произвести тестирование программы для разных размеров обрабатываемых файлов изображений. Для тестирования взять файлы размерами: 2560 x 1920, 3200 x 2400, 5120 x 3840. Получить среднее значение работы процедуры обработки каждого изображения при троекратном перезапуске программы.
Загрузить цветное изображение.
Получить значения интенсивности I_v  =(〖Red〗_ν+〖Green〗_ν+〖Blue〗_ν)/3,                                 
где I_v  – интенсивность пикселя v, 〖Red〗_ν – значение красной компоненты пикселя ν, 〖Green〗_ν– значение зелёной компоненты пикселя ν, 〖Blue〗_ν– значение синей компоненты пикселя ν.
Установить значение скалярной величины - порога Threshold (любое число от 1 до 255). Те значения интенсивности, которые меньше порогового значения установить в 0, а которые больше Threshold установить в 1.
Выполнить операцию наращивания (диляции/ дилатации) [https://habr.com/ru/post/113626/ или https://intuit.ru/studies/courses/10621/1105/lecture/17989?page=4] над полученной матрицей из 0 и 1. Примечание: Нужно задать шаг наращивания (любое значение от 1, 2 или 3). Получить изображение из результата путем установки вместо значений 0 – пикселей черного цвета (0, 0, 0), и вместо значений 1 – пикселей белого цвета. Сохранить результат в файл.
Произвести оценку производительности алгоритма обработки указанных изображений для 2, 4, 6, 8, 10, 12, 14 и 16 потоков на CPU.


Для решения задачи применялся подход с доступом к памяти.