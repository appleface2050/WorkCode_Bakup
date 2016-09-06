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

#pragma once

class CCommand
{
public:
	typedef void Execute_t(const CCommand &command, const vector<PCWSTR> &arguments);

private:
	PCWSTR _name;
	PCWSTR _usage;
	PCWSTR _info;
	PCWSTR _example;
	unsigned int _leagalArgumentsCount;
	Execute_t *_func;
	
	void Validate(const vector<PCWSTR> &arguments) const;

public:
	CCommand(PCWSTR name, PCWSTR usage, PCWSTR info, PCWSTR example, int leagalArgumentsCount, Execute_t *func);
	
	void Execute(const vector<PCWSTR> &arguments) const { Validate(arguments); _func(*this, arguments); {} }
	PCWSTR GetName() const { return _name; }
	
	void DisplayUsage() const;
	void DisplayHelpInformation() const;
};


#define COMMAND(Name, Usage, Info, Example, LeagalArgumentsCount, Function) \
	CCommand __command##Function(Name, Usage, Info, Example, LeagalArgumentsCount, &Function)

void ShowHelp(const CCommand &command, const vector<PCWSTR> &arguments);

class CCommandLineInterpreter
{
	friend void ShowHelp(const CCommand &command, const vector<PCWSTR> &arguments);

private:
	static vector<const CCommand *> _commands;
	typedef vector<const CCommand *>::iterator commandIterator_t;

	CCommandLineInterpreter(void) {}
	CCommandLineInterpreter(const CCommandLineInterpreter &) {}
	
	static bool CCommandLineInterpreter::CompareCommand(PCWSTR cmd1, PCWSTR cmd2);
	static commandIterator_t FindCommand(PCWSTR cmdName);

public:
	static void Register(const CCommand *command);
	static void Execute(int argc, _TCHAR* argv[]);
};