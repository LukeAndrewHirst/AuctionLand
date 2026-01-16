import { Button } from "flowbite-react";
import { useParamsStore } from "../hooks/useParamsStore"
import Heading from "./Heading";

type Props = {
    title?: string
    subtitle?: string,
    showReset?: boolean
}

export default function EmptyFilter({title="No Results Found", subtitle="Try adjust your filters", showReset} : Props) {
  const reset = useParamsStore(state => state.reset);

  return (
    <div className="flex flex-col gap-2 items-center justify-center h-[40vh] shadow-lg">
        <Heading title={title} subtitle={subtitle} />
        <div className="mt-4">
            {showReset && (
                <Button color={'red'} outline onClick={reset}>Remove Filters</Button>
            )}
        </div>
    </div>
  )
}
