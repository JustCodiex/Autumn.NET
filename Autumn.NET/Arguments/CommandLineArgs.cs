using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autumn.Arguments;

public class CommandLineArgs {

    protected readonly string[] args;

    public CommandLineArgs(string[] arguments) {
        this.args = arguments;
    }

    public virtual bool Parse() {
        return true;
    }

}
