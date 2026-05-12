PDF_OUT = build/report.pdf
SOURCES = metadata.yaml $(sort $(wildcard sections/*.md))

all:
	pandoc $(SOURCES) -o $(PDF_OUT) --extract-media=

clean:
	rm $(PDF_OUT)