function DoStuff(){
	console.log(_s("Dra och släpp de kategorierna du vill lägga till i trädet till vänster"));
	console.log(eval("_s(\"Nej\")", {
		test: 3
	}));

	console.log(_s("fail"));
	console.log(_s("Hej")); console.log(_s("Nej")); console.log(_s("Rawr"));
	_s("Visar statistik för $__count$ artiklar.", {
		__count: 3
	});
	_s("$__count$ minuter sedan", {
		__count: 7
	});
}