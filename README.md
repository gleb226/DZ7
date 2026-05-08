# System Programming - Processes (Module 2)

This solution contains a parent application that starts a child process and demonstrates:

- Task 1: start a child process, wait for it to finish, print exit code
- Task 2: start a child process, then either wait for it or terminate it
- Task 3: start a child process and pass command-line arguments (two numbers + operation)
- Task 4: start a child process and pass command-line arguments (file path + word to search)

## Project Structure

- `ProcessesModule2.sln`
- `src/ParentApp` - parent process (launcher)
- `src/ChildApp` - child process (worker)

## Build

From the solution folder:

`dotnet build`

## Run (Interactive)

`dotnet run --project src/ParentApp`

## Run (Non-interactive)

Task 1:

`dotnet run --project src/ParentApp -- task1`

Task 2:

`dotnet run --project src/ParentApp -- task2 wait`

`dotnet run --project src/ParentApp -- task2 kill`

Task 3:

`dotnet run --project src/ParentApp -- task3 7 3 +`

Task 4:

`dotnet run --project src/ParentApp -- task4 "A:\path\file.txt" bicycle`

## ChildApp Commands (Reference)

You normally run the child through `ParentApp`, but the child supports these commands:

- `exitcode <code> [delayMs]`
- `calc <a> <b> <op>`
- `search <filePath> <word>`
