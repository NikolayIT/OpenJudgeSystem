#include<iostream>
#include<random>
#include<unordered_map>
#include<vector>
#include<algorithm>

void nptr(int x)
{
	std::cout<<"This function shouldn't be called\n";
}
void nptr(char*x)
{
	std::cout<<"This function should be called\n";
	if(x) std::cout<<"... but something is wrong!\n";
}

constexpr long long fib(int x)
{
	return x<2?1:fib(x-1)+fib(x-2);
}

int main()
{
	std::cout<<"If the program freezes now it's bad\n";
	const long long bigfib=fib(80);
	std::cout<<"It didn't froze\n";
	std::cout<<bigfib<<'\n';

	std::mt19937 gen{std::random_device{}()};

	std::vector<int> v;
	std::cout<<"Initialised\n";

	v.resize((1<<18)/sizeof(int));
	std::cout<<"Allocated\n";

	for(auto &x:v) x=gen();
	std::cout<<"Filled\n";

	decltype(v) v2=v;
	std::cout<<"Copied\n";
	if(v.size()==v2.size()) std::cout<<"It's okay\n";
	else std::cout<<"It's not okay\n";
	v.clear();

	v=std::move(v2);
	std::cout<<"Moved\n";
	if(v2.size()==0) std::cout<<"It's okay\n";
	else std::cout<<"It's not okay\n";

	v.resize(1<<12);
	v.shrink_to_fit();
	v2.shrink_to_fit();

	std::sort(std::begin(v), std::end(v), [](int a, int b){
		return abs(a)>abs(b);
	});
	std::cout<<"Sorted\n";

	std::shuffle(std::begin(v), std::end(v), gen);
	std::cout<<"Shuffled\n";

	std::unordered_map<int, char> um = {{1, 'a'}, {6, 'b'}, {2, 'x'}};
	std::uniform_int_distribution<char> letters('a', 'z');
	for(auto x:v)
		um.insert({x, letters(gen)});

	nptr(nullptr);

	std::vector<std::vector<char>> vvc={
		{'a', 'b', 'c'},
		{'d', 'e', 'f'},
		{'g', 'h', 'i'}
	};

	std::tuple<int, char, std::vector<int>> tup;
	int x;
	v.resize(42);
	std::get<0>(tup) = 42;
	std::tie(x, std::ignore, v) = tup;
	if(x == 42)
		std::cout<<"Perfect!\n";
	else std::cout<<"Oh, noo! Something is wrong :(\n";

	auto tup2 = std::tuple_cat(tup, std::make_tuple((double)3.5), tup, std::tie(x, x));
}
