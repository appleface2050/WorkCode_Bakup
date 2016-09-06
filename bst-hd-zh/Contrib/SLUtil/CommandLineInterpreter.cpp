// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

#include "StdAfx.h"
#include "CommandLineInterpreter.h"

CCommand::CCommand(PCWSTR name, PCWSTR usage, PCWSTR info, PCWSTR example, int leagalArgumentsCount, Execute_t *func) : 
	  _name(name), _usage(usage), _info(info), _example(example), _leagalArgumentsCount(leagalArgumentsCount), _func(func) 
{
	CCommandLineInterpreter::Register(this);
}

void CCommand::DisplayUsage() const
{
	wcout << _usage << endl;
}

void CCommand::DisplayHelpInformation() const
{
	wcout << L"Help for " << _name << L":" << endl
		<<_info << endl << _usage << endl
		<< L"Example: " << _example << endl;
}

void CCommand::Validate(const vector<PCWSTR> &arguments) const
{
	if (arguments.size() < _leagalArgumentsCount)
	{
		wcerr << _name << L": Missing arguments" << endl;
		DisplayUsage();
		exit(3);
	}
}


vector<const CCommand *> CCommandLineInterpreter::_commands;


bool CCommandLineInterpreter::CompareCommand(PCWSTR cmd1, PCWSTR cmd2)
{
	int length = min(lstrlenW(cmd1), lstrlenW(cmd2));
	return CompareString(LOCALE_NEUTRAL, NORM_IGNORECASE, cmd1, length, cmd2, length) == CSTR_EQUAL;
}

void CCommandLineInterpreter::Register(const CCommand *command)
{
	_commands.push_back(command);
}

CCommandLineInterpreter::commandIterator_t CCommandLineInterpreter::FindCommand(PCWSTR cmdName)
{
	commandIterator_t cmdIter = find_if(_commands.begin(), _commands.end(), 
		bind(&CCommandLineInterpreter::CompareCommand, bind(&CCommand::GetName, _1), cmdName));
	return cmdIter;
}


void CCommandLineInterpreter::Execute(int argc, _TCHAR* argv[])
{
	PCWSTR commandName = L"?";
	
	if (argc >= 2) //no empty command
	{
		commandName = argv[1];
	}

	commandIterator_t cmdIter = FindCommand(commandName);
	if (cmdIter == _commands.end())
	{
		wcerr << L"Bad command, ? for command list" << endl;
		exit(1);
	}
	const CCommand *command = *cmdIter;

	vector<PCWSTR> arguments(max(argc - 2, 0));

	if (argc > 2) //if there are arguments
		copy(&argv[2], &argv[argc], arguments.begin());
	
	try
	{
		command->Execute(arguments);
	}
	catch (...)
	{
		wcerr << command->GetName() << L" execution failed." << endl;
		exit(2);
	}
}

void ShowHelp(const CCommand &command, const vector<PCWSTR> &arguments)
{
	if (arguments.size() == 0)
	{
		for_each(CCommandLineInterpreter::_commands.begin(), CCommandLineInterpreter::_commands.end(),
			bind(&CCommand::DisplayUsage, _1));
		return;
	}
	//else
	
	CCommandLineInterpreter::commandIterator_t cmdIter = CCommandLineInterpreter::FindCommand(arguments[0]);
	if (cmdIter == CCommandLineInterpreter::_commands.end())
	{
		wcerr << L"Bad command, ? for command list" << endl;
		exit(1);
	}
	(*cmdIter)->DisplayHelpInformation();
}

COMMAND(L"?", L"SLUtil ? [CommandName]", L"Show SLUtil help", L"SLUtil ? Create", 0, ShowHelp);